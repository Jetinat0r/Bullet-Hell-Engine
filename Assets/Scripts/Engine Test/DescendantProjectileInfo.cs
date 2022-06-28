using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Descendant Projectile Info Effect", menuName = "ScriptableObjects/Projectile Effects/Descendant Projectile Info Effect")]
public class DescendantProjectileInfo : ProjectileEffect
{
    //The name of the type of this specific effect
    public static string TYPE = "Spawner_Info";

    //This object is designed purely to hold data for things that need it, and as such will only be generated when needed
    public List<SpawnerEffect> spawnerEffects;

    //Used to force the projectileEffectType to TYPE. Can still be changed and it's ultimately up to the dev to decide how to do this, but this is a reasonable way to do it
    private void Awake()
    {
        projectileEffectType = TYPE;
    }

    //Used to reset local values on copy
    public override void Init()
    {
        //Here jic Awake fails to set it
        projectileEffectType = TYPE;

        spawnerEffects = new List<SpawnerEffect>();
        hasAppliedEffects = false;
    }

    public override void Copy(ProjectileEffect oldEffect)
    {
        DescendantProjectileInfo oE = (DescendantProjectileInfo)oldEffect;
        spawnerEffects = oE.spawnerEffects;

        base.Copy(oldEffect);
    }

    //Assigns effects to the correct places in Projectile's sequence of events
    public override void AddEffects(Projectile projectile)
    {
        
    }

    //For effects that synergize / work with other effects
    public override void LateAddEffects(Projectile projectile)
    {

    }

    //Removes the effects assigned in AddEffects()
    public override void RemoveEffects(Projectile projectile)
    {

    }
}
