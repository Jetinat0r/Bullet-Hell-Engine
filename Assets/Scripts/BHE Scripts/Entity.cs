using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for all entities. Applies movement and special behaviors, handles lifetime and collisions
public class Entity : MonoBehaviour
{
    //Components from the gameobject that will always exist and may be of use
    [HideInInspector]
    public Rigidbody2D rb;
    [HideInInspector]
    public SpriteRenderer spriteRenderer;

    //Things set in the editor/before the entity is considered for use
    public string entityType = "DEFAULT_PROJECTILE";
    public ColliderType colliderType = ColliderType.CIRCLE;
    //public Sprite entityGraphics;

    //Things set by the spawner before initialization
    public bool isLaser = false;

    //Things set by the spawner, but may be changed by Behaviors as well
    public List<EntityBehaviour> entityBehaviors = new List<EntityBehaviour>();
    public float damage = 0f;
    public float speed = 0f;

    //Things impacted by the Behaviors attatched
    public Vector3 lastPos;
    public Vector3 nextPos;

    //Things that the entities are allowed to hit (I'm unsure what I want to use here, so I'm leaving my options open
    public List<string> targetTags = new List<string>();
    public List<string> targetTeams = new List<string>();

    #region Methods that EntityBehavior can Influence
    public delegate void UseEntityBehavior(Entity entity);
    //onTriggerEnterEvents for when the entity collides with anything it is allowed to
    public UseEntityBehavior onTriggerEnterEvents = null;
    //onTriggerExitEvents for when the entity stops colliding with anything it is allowed to
    public UseEntityBehavior onTriggerExitEvents = null;
    //onTargetEvents for changing how the entity decides its target
    public UseEntityBehavior onTargetEvents = null;
    //onMoveCalculationEvents for changing how the entity will move towards its target
    public UseEntityBehavior onMoveCalculationEvents = null;
    //postMoveEvents for behavior after the entity moves
    public UseEntityBehavior postMoveEvents = null;
    //onDeathEvents for how the entity behaves when it dies
    //**
    //NOTE: EntityBehaviors are responsible for calling Disable on the entity
    //**
    public UseEntityBehavior onDeathEvents = null;
    //customEvents for when you want to get spicy w/ how the entity behaves. Use at your own risk
    public UseEntityBehavior customEvents = null;
    #endregion

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetupNewPrefab(string _entityType, ColliderType _colliderType, Sprite _sprite = null, Color? _color = null)
    {
        entityType = _entityType;
        colliderType = _colliderType;

        if (_sprite != null)
        {
            spriteRenderer.sprite = _sprite;
        }

        if(_color != null)
        {
            spriteRenderer.color = (Color)_color;
        }
    }

    public void HijackEntity(List<EntityBehaviour> projBehaviors, float _damage = 0f, float _speed = 0f)
    {
        foreach(EntityBehaviour pe in projBehaviors)
        {
            EntityBehaviour newEntityBehavior = BehaviorManager.instance.GetEntityBehavior(pe.EntityBehaviorName);
            newEntityBehavior.Copy(pe);

            entityBehaviors.Add(newEntityBehavior);
        }

        damage = _damage;
        speed = _speed;
    }

    //Sets up initial variables and starts the entity's behavior
    public void Initialize()
    {
        //Initialize lastPos and targetPos jic a component wants to do something with that in onMoveEvents
        lastPos = rb.position;
        nextPos = rb.position;

        if (isLaser)
        {
            ConvertToLaser();
        }

        //Add all custom entity behaviors
        foreach(EntityBehaviour pe in entityBehaviors)
        {
            pe.AddBehaviors(this);
        }

        //Add late entity behaviors
        foreach(EntityBehaviour pe in entityBehaviors)
        {
            pe.LateAddBehaviors(this);
        }
    }

    public void Disable()
    {
        //Play death behaviors
        onDeathEvents?.Invoke(this);

        List<EntityBehaviour> toRemove = new();
        //Undo all non-permanent custom entity behaviors
        foreach (EntityBehaviour pe in entityBehaviors)
        {
            pe.RemoveBehaviors(this);
            if (!pe.isPermanent)
            {
                toRemove.Add(pe);
            }
        }

        foreach(EntityBehaviour pe in toRemove)
        {
            entityBehaviors.Remove(pe);
            BehaviorManager.instance.DeactivateEntityBehavior(pe);
        }

        //Readies the entity for re-use
        EntityManager.instance.DeactivateEntity(this);
    }

    public void AddBehaviors(List<EntityBehaviour> entityBehaviors)
    {
        int oldCount = entityBehaviors.Count;

        foreach(EntityBehaviour pe in entityBehaviors)
        {
            EntityBehaviour newBehavior = BehaviorManager.instance.GetEntityBehavior(pe.EntityBehaviorName);
            newBehavior.Copy(pe);

            entityBehaviors.Add(newBehavior);

            //TODO: Add handling for Lasers
        }

        for(int i = 0; i < oldCount; i++)
        {
            entityBehaviors[i].UpdateBehaviors(this);
        }
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

        //Use jic an behavior does something like make a laser trail behind the entity
        postMoveEvents?.Invoke(this);

        //Use jic an behavior wants to be cool
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
