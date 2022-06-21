using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEffect : ScriptableObject
{
    [Header("Base Vars")]
    //The key for this projectile effect. Should never be changed after being cached in CachedBHEResources
    public string projectileEffectType = "DEFAULT_PROJECTILE_EFFECT";
    //Prevents the component from being removed from a projectile when that projectile's Disable() is called
    //WARNING: effects are allowed to be added to the same projectile multiple times. this may result in unexpected behavior such as projectiles moving twice as far in one step
    //I recommend avoiding this altogether
    public bool isPermanent = false;
    /// <summary> 
    /// Determines if this effect should be passed to spawners generated by the attatched projectile
    /// 
    /// Examples of effects to not pass to children: projectile splitting effects (infinite projectiles is bad!)
    /// 
    /// A value of -1 will pass the effect every time (logic is: inherit when != 0)
    /// This needs to be decremented every time a projectile adds a new copy of the effect TO a SPAWNER, not from the spawner when added to a projectile or anywhere else
    /// </summary>
    public int generationsToInheritEffect = -1;
    protected bool hasAppliedEffects = false;

    //Used to reset local values on copy
    public virtual void Init()
    {
        hasAppliedEffects = false;
    }

    //Assigns effects to the correct places in Projectile's sequence of events
    public virtual void AddEffects(Projectile projectile)
    {
        if (!hasAppliedEffects)
        {
            //Apply Effects
            //Debug.Log("Applying Effects!");
            hasAppliedEffects = true;
        }
    }

    //Removes the effects assigned in AddEffects()
    public virtual void RemoveEffects(Projectile projectile)
    {
        if (isPermanent)
        {
            //Debug.Log("Permanent effect, not removing");
            return;
        }

        //Remove Effects
        //Debug.Log("Removing Effects");

        //TODO: Maybe send the component back to a pool? Let's see if it's necessary first
    }

    //TODO: figure this out lol I really want lasers
    public virtual T ConvertToLaserEffect<T>()
    {
        return default(T);
    }
}
