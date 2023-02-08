using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Create Spawner On Death Behavior", menuName = "ScriptableObjects/Entity Behaviors/Create Spawner On Death Entity Behavior")]
public class CreateSpawnerOnDeathEntityBehavior : EntityBehaviour
{
    public new const string EntityBehaviorType = "Create_Spawner_On_Death";
    public override string GetEntityBehaviorType() => EntityBehaviorType;

    [Space(10)]
    [Header("Create Spawner On Death Vars")]
    public EntitySpawner SpawnerPrefab;

    public List<SpawnerBehavior> spawnerBehaviors = null;

    //Used to reset local values on copy
    public override void Init()
    {
        spawnerBehaviors = null;
        hasAppliedBehaviors = false;
    }

    public override void Copy(EntityBehaviour oldBehavior)
    {
        CreateSpawnerOnDeathEntityBehavior oE = (CreateSpawnerOnDeathEntityBehavior)oldBehavior;
        SpawnerPrefab = oE.SpawnerPrefab;

        spawnerBehaviors = oE.spawnerBehaviors;

        base.Copy(oldBehavior);
    }

    //Assigns behaviors to the correct places in Entity's sequence of events
    public override void AddBehaviors(Entity entity)
    {
        if (!hasAppliedBehaviors)
        {
            //Apply Behaviors
            //Debug.Log("Applying Behaviors!");

            entity.onDeathEvents += CreateSpawner;

            hasAppliedBehaviors = true;
        }
    }

    //For behaviors that synergize / work with other behaviors
    public override void LateAddBehaviors(Entity entity)
    {
        if (!hasAppliedLateBehaviors)
        {
            foreach (EntityBehaviour behavior in entity.entityBehaviors)
            {
                //If there is a SpawnerBehaviorContainerEntityBehavior available on the entity, grab it's spawner behaviors
                if (behavior.GetEntityBehaviorType() == SpawnerBehaviorContainerEntityBehavior.EntityBehaviorType)
                {
                    SpawnerBehaviorContainerEntityBehavior spawnerInfo = behavior as SpawnerBehaviorContainerEntityBehavior;

                    spawnerBehaviors = new List<SpawnerBehavior>();
                    foreach (SpawnerBehavior se in spawnerInfo.spawnerBehaviors)
                    {
                        SpawnerBehavior newSpawnerBehavior = BehaviorManager.instance.GetSpawnerBehavior(se.SpawnerBehaviorName);
                        newSpawnerBehavior.Copy(se);
                        spawnerBehaviors.Add(newSpawnerBehavior);
                    }

                    break;
                }
            }
        }
    }

    //Removes the behaviors assigned in AddBehaviors()
    public override void RemoveBehaviors(Entity entity)
    {
        if (isPermanent)
        {
            return;
        }

        //Remove Behaviors
        //Debug.Log("Removing Behaviors");
        
        entity.onDeathEvents -= CreateSpawner;
    }

    //TODO: figure this out lol I really want lasers
    public override T ConvertToLaserBehavior<T>()
    {
        return default(T);
    }

    private void CreateSpawner(Entity entity)
    {
        //TODO: Enable pooling? and caching of spawner prefabs
        EntitySpawner newSpawner = Instantiate(SpawnerPrefab, entity.transform.position, entity.transform.rotation);

        newSpawner.gameObject.SetActive(true);
        newSpawner.Init(_enableAfterInit:false, _destroyOnDisable:true);

        if (spawnerBehaviors != null)
        {
            //Can't use newSpawner.AddSpawnerBehaviors() because I need to decrement generationsToInherit. Could I have done this better? sure. do i care enough to fix it? nope
            foreach (SpawnerBehavior se in spawnerBehaviors)
            {
                if(se.generationsToInheritBehavior != 0)
                {
                    SpawnerBehavior newSpawnerBehavior = BehaviorManager.instance.GetSpawnerBehavior(se.SpawnerBehaviorName);
                    newSpawnerBehavior.Copy(se);
                    newSpawnerBehavior.generationsToInheritBehavior--;

                    newSpawner.spawnerBehaviors.Add(newSpawnerBehavior);
                }
            }
        }

        //Don't need a != null check here bc of how they are assigned
        //Can't use newSpawner.AddEntityBehaviors() because I need to decrement generationsToInherit. Could I have done this better? sure. do i care enough to fix it? nope
        foreach (EntityBehaviour pe in entity.entityBehaviors)
        {
            if(pe.generationsToInheritBehavior != 0)
            {
                EntityBehaviour newProjBehavior = BehaviorManager.instance.GetEntityBehavior(pe.EntityBehaviorName);
                newProjBehavior.Copy(pe);
                newProjBehavior.generationsToInheritBehavior--;

                newSpawner.entityBehaviors.Add(newProjBehavior);
            }
        }

        newSpawner.Enable();
    }
}
