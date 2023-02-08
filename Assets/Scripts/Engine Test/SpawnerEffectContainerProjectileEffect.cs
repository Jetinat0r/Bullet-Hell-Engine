using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This
//
//Behavior is a container for its parent spawner behaviors
[CreateAssetMenu(fileName = "New Spawner Behavior Container Entity Behavior", menuName = "ScriptableObjects/Entity Behaviors/Spawner Behavior Container")]
public class SpawnerBehaviorContainerEntityBehavior : EntityBehaviour
{
    public new const string EntityBehaviorType = "Spawner_Behavior_Container_Entity_Behavior";
    public override string GetEntityBehaviorType() => EntityBehaviorType;

    //This object is designed purely to hold data for things that need it, and as such will only be generated when needed
    public List<SpawnerBehavior> spawnerBehaviors;

    //Used to reset local values on copy
    public override void Init()
    {
        //If there is spawner behaviors, they need returned to the cache before we can use the list again
        //Don't need to remove them since that happens right after
        if (spawnerBehaviors != null && spawnerBehaviors.Count != 0)
        {
            foreach (SpawnerBehavior se in spawnerBehaviors)
            {
                BehaviorManager.instance.DeactivateSpawnerBehavior(se);
            }
        }


        spawnerBehaviors = new List<SpawnerBehavior>();
        hasAppliedBehaviors = false;
    }

    public override void Copy(EntityBehaviour oldBehavior)
    {
        SpawnerBehaviorContainerEntityBehavior oE = (SpawnerBehaviorContainerEntityBehavior)oldBehavior;
        spawnerBehaviors = oE.spawnerBehaviors;

        base.Copy(oldBehavior);
    }

    //Assigns behaviors to the correct places in Entity's sequence of events
    public override void AddBehaviors(Entity entity)
    {
        
    }

    //For behaviors that synergize / work with other behaviors
    public override void LateAddBehaviors(Entity entity)
    {

    }

    //Removes the behaviors assigned in AddBehaviors()
    public override void RemoveBehaviors(Entity entity)
    {

    }
}
