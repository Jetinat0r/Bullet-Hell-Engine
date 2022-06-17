using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for all projectiles. Applies movement and special effects, handles lifetime and collisions
public class Projectile : MonoBehaviour
{
    //Components from the gameobject that will always exist and may be of use
    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    //Things set in the editor/before the projectile is considered for use
    public string projectileType = "DEFAULT_PROJECTILE";
    //public Sprite projectileGraphics;

    //Things set by the spawner before initialization
    public bool isLaser = false;

    //Things set by the spawner, but may be changed by Effects as well
    public List<ProjectileEffect> projectileEffects = new List<ProjectileEffect>();
    public float damage = 0f;
    public float speed = 0f;

    //Things impacted by the Effects attatched
    public Vector3 lastPos;
    public Vector3 nextPos;

    //Things that the projectiles are allowed to hit (I'm unsure what I want to use here, so I'm leaving my options open
    public List<string> targetTags = new List<string>();
    public List<string> targetTeams = new List<string>();

    //Properties that ProjectileEffect can influence
    public delegate void UseProjectileEffect(Projectile projectile);
    //onTriggerEnterEvents for when the projectile collides with anything it is allowed to
    public UseProjectileEffect onTriggerEnterEvents = null;
    //onTriggerExitEvents for when the projectile stops colliding with anything it is allowed to
    public UseProjectileEffect onTriggerExitEvents = null;
    //onTargetEvents for changing how the projectile decides its target
    public UseProjectileEffect onTargetEvents = null;
    //onMoveCalculationEvents for changing how the projectile will move towards its target
    public UseProjectileEffect onMoveCalculationEvents = null;
    //postMoveEvents for behavior after the projectile moves
    public UseProjectileEffect postMoveEvents = null;
    //onDeathEvents for how the projectile behaves when it dies
    //**
    //NOTE: ProjectileEffects are responsible for calling Disable on the projectile
    //**
    public UseProjectileEffect onDeathEvents = null;
    //customEvents for when you want to get spicy w/ how the projectile behaves. Use at your own risk
    public UseProjectileEffect customEvents = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void HijackProjectile(List<ProjectileEffect> projEffects, float _damage = 0f, float _speed = 0f)
    {
        foreach(ProjectileEffect pe in projEffects)
        {
            ProjectileEffect clone = Instantiate(pe);
            clone.Init();
            projectileEffects.Add(clone);
        }

        damage = _damage;
        speed = _speed;
    }

    //Sets up initial variables and starts the projectile's behavior
    public void Initialize()
    {
        //Initialize lastPos and targetPos jic a component wants to do something with that in onMoveEvents
        lastPos = rb.position;
        nextPos = rb.position;

        if (isLaser)
        {
            ConvertToLaser();
        }

        //Add all custom projectile effects
        foreach(ProjectileEffect pe in projectileEffects)
        {
            pe.AddEffects(this);
        }
    }

    public void Disable()
    {
        //Play death effects
        onDeathEvents?.Invoke(this);

        List<ProjectileEffect> toRemove = new();
        //Undo all non-permanent custom projectile effects
        foreach (ProjectileEffect pe in projectileEffects)
        {
            pe.RemoveEffects(this);
            if (!pe.isPermanent)
            {
                toRemove.Add(pe);
            }
        }

        foreach(ProjectileEffect pe in toRemove)
        {
            projectileEffects.Remove(pe);
        }

        //Readies the projectile for re-use
        ProjectileManager.instance.DeactivateProjectile(this);
    }

    private void FixedUpdate()
    {
        //Find a target
        onTargetEvents?.Invoke(this);

        //Initialize nextPos in preperation for movement calculation events
        nextPos = rb.position;
        //Determine movement
        onMoveCalculationEvents?.Invoke(this);

        //Update lastPos
        lastPos = rb.position;
        //Move to the determined position
        rb.position = nextPos;

        //Use jic an effect does something like make a laser trail behind the projectile
        postMoveEvents?.Invoke(this);

        //Use jic an effect wants to be cool
        customEvents?.Invoke(this);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //TODO: Check if it is a valid hit (search for IHittable Compenent or something, then compare either tags or teams or something)
        onTriggerEnterEvents?.Invoke(this);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //TODO: Check if it is a valid hit (search for IHittable Compenent or something, then compare either tags or teams or something)
        onTriggerExitEvents?.Invoke(this);
    }

    private void ConvertToLaser()
    {
        throw new NotImplementedException();
    }
}
