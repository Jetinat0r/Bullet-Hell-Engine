using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEffect : ScriptableObject
{
    [Header("Base Vars")]
    //The difference between these two vars is that Type is used for all prefabs of an effect, i.e. Rotate_Effect, while Name is used for individual setups, i.e. Rotate_45_Clockwise_Effect
    //Type is designed for other effects to grab this one, even if the values are different (i.e. an effect can grab a Rotate_Effect whether it's Rotate_45_Clockwise or Rotate_15_CounterClockwise)
    //Name is used by the object pooler to grab specific prefabs of effects

    //A tag for the type of projectile effect it is. Used so other effects can locate this one. Should not be changed after caching
    public string projectileEffectType = "DEFAULT_PROJECTILE_EFFECT_TYPE";

    //The key for this specific setup of the projectile effect. Should never be changed after being cached in CachedBHEResources
    public string projectileEffectName = "DEFAULT_PROJECTILE_EFFECT_NAME";



    //Prevents the component from being removed from a projectile when that projectile's Disable() is called
    //WARNING: effects are allowed to be added to the same projectile multiple times. this may result in unexpected behavior such as projectiles moving twice as far in one step
    //          I recommend avoiding this altogether
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
    protected bool hasAppliedLateEffects = false;

    //Used to reset local values on copy
    public virtual void Init()
    {
        hasAppliedEffects = false;
    }

    //Copy is used for values changed after initialization relative to a specific copy of the effect (i.e an effect that contains a list of objects in the scene)
    public virtual void Copy(ProjectileEffect oldEffect)
    {
        //projectileEffectType = oldEffect.projectileEffectType;
        //projectileEffectName = oldEffect.projectileEffectName;

        isPermanent = oldEffect.isPermanent;

        generationsToInheritEffect = oldEffect.generationsToInheritEffect;

        hasAppliedEffects = false;
        hasAppliedLateEffects = false;
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

    //For effects that synergize / work with other effects
    public virtual void LateAddEffects(Projectile projectile)
    {
        //TODO: Standardize whether the = true appears inside or outside the if
        if (!hasAppliedLateEffects)
        {
            //Apply late effects
            hasAppliedLateEffects = true;
        }
    }

    //For effects when a new effect is added via an outside actor after LateAddEffects
    public virtual void UpdateEffects(Projectile projectile)
    {

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
    }

    //TODO: figure this out lol I really want lasers
    public virtual T ConvertToLaserEffect<T>()
    {
        return default(T);
    }
}
