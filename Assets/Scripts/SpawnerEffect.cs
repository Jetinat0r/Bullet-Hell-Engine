using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerEffect : ScriptableObject
{
    [Header("Base Vars")]
    //The tag which it is retrieved by
    public string spawnerEffectType = "DEFAULT_SPAWNER_EFFECT";

    /// <summary> 
    /// Determines if this effect should be passed to spawners generated by projectiles it creates
    /// 
    /// I can't think of any cases that would make it so that 
    /// 
    /// A value of -1 will pass the effect every time (logic is: inherit when != 0)
    /// This needs to be decremented every time a projectile adds a new copy of the effect TO a SPAWNER, not from the spawner when added to a projectile or anywhere else
    /// </summary>
    public int generationsToInheritEffect = -1;
    protected bool hasAppliedEffects = false;

    public virtual void Init()
    {
        hasAppliedEffects = false;
    }

    public virtual void AddEffects(ProjectileSpawner spawner)
    {
        if (!hasAppliedEffects)
        {

        }

        hasAppliedEffects = true;
    }

    public virtual void RemoveEffects(ProjectileSpawner spawner)
    {
        hasAppliedEffects = false;
    }
}
