using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultProjectileEffects : ProjectileEffect
{
    public float lifetime = 2.5f;
    private float timeAlive = 0f;

    
    #region Constructors

    public DefaultProjectileEffects(float _lifetime = 2.5f, bool _isPermanent = false, int _childrenToInheritEffect = 0) :
        base(_isPermanent, _childrenToInheritEffect)
    {
        lifetime = _lifetime;
    }

    public DefaultProjectileEffects(DefaultProjectileEffects other) : base(other)
    {
        lifetime = other.lifetime;
        timeAlive = 0f;
    }

    public override ProjectileEffect Clone()
    {
        return new DefaultProjectileEffects(this);
    }

    #endregion

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
