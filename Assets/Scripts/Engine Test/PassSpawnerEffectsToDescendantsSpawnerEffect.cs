using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This Spawner Effect creates a projectile effect that tells those projectiles what spawner effects the original one had
[CreateAssetMenu(fileName = "New Pass Spawner Effects To Descendants Spawner Effect", menuName = "ScriptableObjects/Spawner Effects/Pass Spawner Effects To Descendants")]
public class PassSpawnerEffectsToDescendantsSpawnerEffect : SpawnerEffect
{
    //[Space(10)]
    //[Header("Descendant Spawner Info Vars")]
    //public List<SpawnerEffect> spawnerEffects = new List<SpawnerEffect>();
    public SpawnerEffectContainerProjectileEffect spawnerEffectContainerProjectileEffect;

    public override void Init()
    {
        //spawnerEffects = new List<SpawnerEffect>();
        //descendantProjectileInfo = CreateInstance<DescendantProjectileInfo>();
        if(spawnerEffectContainerProjectileEffect == null)
        {
            spawnerEffectContainerProjectileEffect = EffectManager.instance.GetProjectileEffect(SpawnerEffectContainerProjectileEffect.TYPE) as SpawnerEffectContainerProjectileEffect;
        }

        hasAppliedEffects = false;
    }

    public override void AddEffects(ProjectileSpawner spawner)
    {
        
    }

    public override void LateAddEffects(ProjectileSpawner spawner)
    {
        if (!hasAppliedLateEffects)
        {
            //If there is spawner effects, they need returned to the cache before we can use the list again
            //Don't need to remove them since that happens right after
            if (spawnerEffectContainerProjectileEffect.spawnerEffects != null && spawnerEffectContainerProjectileEffect.spawnerEffects.Count != 0)
            {
                foreach(SpawnerEffect se in spawnerEffectContainerProjectileEffect.spawnerEffects)
                {
                    EffectManager.instance.DeactivateSpawnerEffect(se);
                }
            }


            spawnerEffectContainerProjectileEffect.spawnerEffects = new List<SpawnerEffect>();

            foreach (SpawnerEffect _effect in spawner.spawnerEffects)
            {
                SpawnerEffect newSpawnerEffect = EffectManager.instance.GetSpawnerEffect(_effect.spawnerEffectName);
                newSpawnerEffect.Copy(_effect);
                spawnerEffectContainerProjectileEffect.spawnerEffects.Add(newSpawnerEffect);
            }

            spawner.projectileEffects.Add(spawnerEffectContainerProjectileEffect);
        }

        
        hasAppliedLateEffects = true;
    }

    public override void UpdateEffects(ProjectileSpawner spawner)
    {
        spawnerEffectContainerProjectileEffect.spawnerEffects = new List<SpawnerEffect>();

        foreach (SpawnerEffect _effect in spawner.spawnerEffects)
        {
            SpawnerEffect newSpawnerEffect = EffectManager.instance.GetSpawnerEffect(_effect.spawnerEffectName);
            newSpawnerEffect.Copy(_effect);
            spawnerEffectContainerProjectileEffect.spawnerEffects.Add(newSpawnerEffect);
        }
    }

    public override void RemoveEffects(ProjectileSpawner spawner)
    {
        //Returns spawnerEffectContainerProjectileEffect to cache
        spawner.RemoveProjectileEffects(new List<ProjectileEffect> { spawnerEffectContainerProjectileEffect });
        hasAppliedEffects = false;
    }
}
