using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Descendant Spawner Info Effect", menuName = "ScriptableObjects/Spawner Effects/Descendant Spawner Info Effect")]
public class DescendantSpawnerInfo : SpawnerEffect
{
    //[Space(10)]
    //[Header("Descendant Spawner Info Vars")]
    //public List<SpawnerEffect> spawnerEffects = new List<SpawnerEffect>();
    public DescendantProjectileInfo descendantProjectileInfo;

    public override void Init()
    {
        //spawnerEffects = new List<SpawnerEffect>();
        //descendantProjectileInfo = CreateInstance<DescendantProjectileInfo>();
        if(descendantProjectileInfo != null)
        {
            //TODO: return to cache
        }

        descendantProjectileInfo = CachedBHEResources.instance.GetProjectileEffect(DescendantProjectileInfo.TYPE) as DescendantProjectileInfo;

        hasAppliedEffects = false;
    }

    public override void AddEffects(ProjectileSpawner spawner)
    {
        
    }

    public override void LateAddEffects(ProjectileSpawner spawner)
    {
        if (!hasAppliedLateEffects)
        {
            descendantProjectileInfo.spawnerEffects = new List<SpawnerEffect>();

            foreach (SpawnerEffect _effect in spawner.spawnerEffects)
            {
                SpawnerEffect newSpawnerEffect = CachedBHEResources.instance.GetSpawnerEffect(_effect.spawnerEffectName);
                newSpawnerEffect.Copy(_effect);
                descendantProjectileInfo.spawnerEffects.Add(newSpawnerEffect);
            }

            spawner.projectileEffects.Add(descendantProjectileInfo);
        }

        
        hasAppliedLateEffects = true;
    }

    public override void UpdateEffects(ProjectileSpawner spawner)
    {
        descendantProjectileInfo.spawnerEffects = new List<SpawnerEffect>();

        foreach (SpawnerEffect _effect in spawner.spawnerEffects)
        {
            SpawnerEffect newSpawnerEffect = CachedBHEResources.instance.GetSpawnerEffect(_effect.spawnerEffectName);
            newSpawnerEffect.Copy(_effect);
            descendantProjectileInfo.spawnerEffects.Add(newSpawnerEffect);
        }
    }

    public override void RemoveEffects(ProjectileSpawner spawner)
    {
        //TODO: Return descendantProjectileInfo to cache
        spawner.RemoveProjectileEffects(new List<ProjectileEffect> { descendantProjectileInfo });
        hasAppliedEffects = false;
    }
}
