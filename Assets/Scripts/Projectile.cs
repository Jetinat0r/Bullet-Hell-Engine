using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for all projectiles. Applies movement and special effects, handles lifetime and collisions
public class Projectile : MonoBehaviour
{
    public float speed = 0f;
    public Vector3 lastPos;
    public Vector3 nextPos;
    public string projectileType = "DEFAULT_PROJECTILE";
    public Sprite projectileGraphics;

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
    //NOTE: ProjectileEffects are responsible for calling Disable on the projectile
    public UseProjectileEffect onDeathEvents = null;
    //customEvents for when you want to get spicy w/ how the projectile behaves. Use at your own risk
    public UseProjectileEffect customEvents = null;

    //Sets up initial variables and starts the projectile's behavior
    public void Initialize()
    {
        //Initialize lastPos and targetPos jic a component wants to do something with that in onMoveEvents
        lastPos = transform.position;
        nextPos = transform.position;

        foreach(ProjectileEffect pe in GetComponents<ProjectileEffect>())
        {
            pe.AddEffects(this);
        }
    }

    public void Disable()
    {
        onDeathEvents?.Invoke(this);

        //TODO: Undo all Custom Components, i.e foreach (ProjectileEffect pe in components)
        foreach (ProjectileEffect pe in GetComponents<ProjectileEffect>())
        {
            pe.RemoveEffects(this);
        }

        //Readies the projectile for re-use
        ProjectileManager.instance.DeactivateProjectile(this);
    }

    private void FixedUpdate()
    {
        //Find a target
        onTargetEvents?.Invoke(this);

        //Initialize nextPos in preperation for movement calculation events
        nextPos = transform.position;
        //Determine movement
        onMoveCalculationEvents?.Invoke(this);

        //Update lastPos
        lastPos = transform.position;
        //Move to the determined position
        transform.position = nextPos;
        //TODO: MAYBE do a ray/spherecast between lasPos and targetPos?

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
}
