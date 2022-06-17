using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Default Projectile Effects", menuName = "ScriptableObjects/Projectile Effects/Default Projectile Effects")]
public class DefaultProjectileEffects : ProjectileEffect
{
    [Space(10f)]
    [Header("Default Effect Vars")]
    public float lifetime = 2.5f;
    private float timeAlive = 0f;

    //Used to reset local values on copy
    public override void Init()
    {
        base.Init();
        timeAlive = 0f;
    }

    //Assigns effects to the correct places in Projectile's sequence of events
    public override void AddEffects(Projectile projectile)
    {
        if (!hasAppliedEffects)
        {
            //Apply Effects
            Debug.Log("Applying Effects!");

            projectile.onMoveCalculationEvents += ChangeMovement;
            projectile.customEvents += CustomEvent;

            hasAppliedEffects = true;
        }

        //Initialize lifetime timer
        timeAlive = 0f;
    }

    //Removes the effects assigned in AddEffects()
    public override void RemoveEffects(Projectile projectile)
    {
        if (isPermanent)
        {
            Debug.Log("Permanent effect, not removing");
            return;
        }

        //Remove Effects
        Debug.Log("Removing Effects");

        projectile.onMoveCalculationEvents -= ChangeMovement;
        projectile.customEvents -= CustomEvent;

        //Destroy component after removing effects since it is not permanent
        //Destroy(this);
    }

    private void ChangeMovement(Projectile projectile)
    {
        projectile.nextPos += projectile.transform.right * projectile.speed * Time.fixedDeltaTime;
    }

    private void CustomEvent(Projectile projectile)
    {
        //Kill the projectile if it has reached the end of its lifetime
        timeAlive += Time.fixedDeltaTime;
        if (timeAlive >= lifetime)
        {
            projectile.Disable();
        }
    }
}
