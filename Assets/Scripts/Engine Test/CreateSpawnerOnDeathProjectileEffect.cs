using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Create Spawner On Death Projectile Effect", menuName = "ScriptableObjects/Projectile Effects/Create Spawner On Death Projectile Effect")]
public class CreateSpawnerOnDeathProjectileEffect : ProjectileEffect
{
    [Space(10)]
    [Header("Create Spawner On Death Vars")]
    public ProjectileSpawner SpawnerPrefab;

    public List<SpawnerEffect> spawnerEffects = null;

    //Used to reset local values on copy
    public override void Init()
    {
        spawnerEffects = null;
        hasAppliedEffects = false;
    }

    public override void Copy(ProjectileEffect oldEffect)
    {
        CreateSpawnerOnDeathProjectileEffect oE = (CreateSpawnerOnDeathProjectileEffect)oldEffect;
        SpawnerPrefab = oE.SpawnerPrefab;

        spawnerEffects = oE.spawnerEffects;

        base.Copy(oldEffect);
    }

    //Assigns effects to the correct places in Projectile's sequence of events
    public override void AddEffects(Projectile projectile)
    {
        if (!hasAppliedEffects)
        {
            //Apply Effects
            //Debug.Log("Applying Effects!");

            projectile.onDeathEvents += CreateSpawner;

            hasAppliedEffects = true;
        }
    }

    //For effects that synergize / work with other effects
    public override void LateAddEffects(Projectile projectile)
    {
        if (!hasAppliedLateEffects)
        {
            foreach (ProjectileEffect effect in projectile.projectileEffects)
            {
                //If there is a DescendantProjectileInfo SpawnerEffect available on the projectile, grab it's 
                if (effect.projectileEffectType == DescendantProjectileInfo.TYPE)
                {
                    DescendantProjectileInfo spawnerInfo = effect as DescendantProjectileInfo;

                    spawnerEffects = new List<SpawnerEffect>();
                    foreach (SpawnerEffect se in spawnerInfo.spawnerEffects)
                    {
                        SpawnerEffect newSpawnerEffect = CachedBHEResources.instance.GetSpawnerEffect(se.spawnerEffectName);
                        newSpawnerEffect.Copy(se);
                        spawnerEffects.Add(newSpawnerEffect);
                    }

                    break;
                }
            }
        }
    }

    //Removes the effects assigned in AddEffects()
    public override void RemoveEffects(Projectile projectile)
    {
        if (isPermanent)
        {
            return;
        }

        //Remove Effects
        //Debug.Log("Removing Effects");
        
        projectile.onDeathEvents -= CreateSpawner;
        //TODO: Maybe send the component back to a pool? Let's see if it's necessary first (NO, DO IN WHAT HOLDS THE EFFECTS)
    }

    //TODO: figure this out lol I really want lasers
    public override T ConvertToLaserEffect<T>()
    {
        return default(T);
    }

    private void CreateSpawner(Projectile projectile)
    {
        //TODO: Enable pooling? and caching of spawner prefabs
        ProjectileSpawner newSpawner = Instantiate(SpawnerPrefab, projectile.transform.position, projectile.transform.rotation);

        newSpawner.gameObject.SetActive(true);
        newSpawner.Init(_enableAfterInit:false, _destroyOnDisable:true);

        if (spawnerEffects != null)
        {
            //Can't use newSpawner.AddSpawnerEffects() because I need to decrement generationsToInherit. Could I have done this better? sure. do i care enough to fix it? nope
            foreach (SpawnerEffect se in spawnerEffects)
            {
                if(se.generationsToInheritEffect != 0)
                {
                    SpawnerEffect newSpawnerEffect = CachedBHEResources.instance.GetSpawnerEffect(se.spawnerEffectName);
                    newSpawnerEffect.Copy(se);
                    newSpawnerEffect.generationsToInheritEffect--;

                    newSpawner.spawnerEffects.Add(newSpawnerEffect);
                }
            }
        }

        //Don't need a != null check here bc of how they are assigned
        //Can't use newSpawner.AddProjectileEffects() because I need to decrement generationsToInherit. Could I have done this better? sure. do i care enough to fix it? nope
        foreach (ProjectileEffect pe in projectile.projectileEffects)
        {
            if(pe.generationsToInheritEffect != 0)
            {
                ProjectileEffect newProjEffect = CachedBHEResources.instance.GetProjectileEffect(pe.projectileEffectName);
                newProjEffect.Copy(pe);
                newProjEffect.generationsToInheritEffect--;

                newSpawner.projectileEffects.Add(newProjEffect);
            }
        }

        newSpawner.Enable();
    }
}
