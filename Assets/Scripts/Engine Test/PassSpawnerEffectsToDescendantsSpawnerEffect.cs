using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This Spawner Behavior creates a entity behavior that tells those entities what spawner behaviors the original one had
[CreateAssetMenu(fileName = "New Pass Spawner Behaviors To Descendants Spawner Behavior", menuName = "ScriptableObjects/Spawner Behaviors/Pass Spawner Behaviors To Descendants")]
public class PassSpawnerBehaviorsToDescendantsSpawnerBehavior : SpawnerBehavior
{
    public new const string SpawnerBehaviorType = "Pass_Spawner_Behaviors_To_Descendants";
    public override string GetSpawnerBehaviorType() => SpawnerBehaviorType;

    //[Space(10)]
    //[Header("Descendant Spawner Info Vars")]
    //public List<SpawnerBehavior> spawnerBehaviors = new List<SpawnerBehavior>();
    public SpawnerBehaviorContainerEntityBehavior spawnerBehaviorContainerEntityBehavior;

    public override void Init()
    {
        //spawnerBehaviors = new List<SpawnerBehavior>();
        //descendantEntityInfo = CreateInstance<DescendantEntityInfo>();
        if(spawnerBehaviorContainerEntityBehavior == null)
        {
            spawnerBehaviorContainerEntityBehavior = BehaviorManager.instance.GetEntityBehavior(SpawnerBehaviorContainerEntityBehavior.EntityBehaviorType) as SpawnerBehaviorContainerEntityBehavior;
        }

        hasAppliedBehaviors = false;
    }

    public override void AddBehaviors(EntitySpawner spawner)
    {
        
    }

    public override void LateAddBehaviors(EntitySpawner spawner)
    {
        if (!hasAppliedLateBehaviors)
        {
            //If there is spawner behaviors, they need returned to the cache before we can use the list again
            //Don't need to remove them since that happens right after
            if (spawnerBehaviorContainerEntityBehavior.spawnerBehaviors != null && spawnerBehaviorContainerEntityBehavior.spawnerBehaviors.Count != 0)
            {
                foreach(SpawnerBehavior se in spawnerBehaviorContainerEntityBehavior.spawnerBehaviors)
                {
                    BehaviorManager.instance.DeactivateSpawnerBehavior(se);
                }
            }


            spawnerBehaviorContainerEntityBehavior.spawnerBehaviors = new List<SpawnerBehavior>();

            foreach (SpawnerBehavior _behavior in spawner.spawnerBehaviors)
            {
                SpawnerBehavior newSpawnerBehavior = BehaviorManager.instance.GetSpawnerBehavior(_behavior.SpawnerBehaviorName);
                newSpawnerBehavior.Copy(_behavior);
                spawnerBehaviorContainerEntityBehavior.spawnerBehaviors.Add(newSpawnerBehavior);
            }

            spawner.entityBehaviors.Add(spawnerBehaviorContainerEntityBehavior);
        }

        
        hasAppliedLateBehaviors = true;
    }

    public override void UpdateBehaviors(EntitySpawner spawner)
    {
        spawnerBehaviorContainerEntityBehavior.spawnerBehaviors = new List<SpawnerBehavior>();

        foreach (SpawnerBehavior _behavior in spawner.spawnerBehaviors)
        {
            SpawnerBehavior newSpawnerBehavior = BehaviorManager.instance.GetSpawnerBehavior(_behavior.SpawnerBehaviorName);
            newSpawnerBehavior.Copy(_behavior);
            spawnerBehaviorContainerEntityBehavior.spawnerBehaviors.Add(newSpawnerBehavior);
        }
    }

    public override void RemoveBehaviors(EntitySpawner spawner)
    {
        //Returns spawnerBehaviorContainerEntityBehavior to cache
        spawner.RemoveEntityBehaviors(new List<EntityBehaviour> { spawnerBehaviorContainerEntityBehavior });
        hasAppliedBehaviors = false;
    }
}
