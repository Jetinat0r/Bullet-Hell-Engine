using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Default Behaviors", menuName = "ScriptableObjects/Entity Behaviors/Default Entity Behaviors")]
public class DefaultEntityBehaviors : EntityBehaviour
{
    public new const string EntityBehaviorType = "Default_Entity_Behaviors";
    public override string GetEntityBehaviorType() => EntityBehaviorType;

    [Space(10f)]
    [Header("Default Behavior Vars")]
    public float lifetime = 2.5f;
    private float timeAlive = 0f;

    //Used to reset local values on copy
    public override void Init()
    {
        base.Init();
        timeAlive = 0f;
    }

    //Assigns behaviors to the correct places in Entity's sequence of events
    public override void AddBehaviors(Entity entity)
    {
        if (!hasAppliedBehaviors)
        {
            //Apply Behaviors
            //Debug.Log("Applying Behaviors!");

            entity.onMoveCalculationEvents += ChangeMovement;
            entity.customEvents += CustomEvent;

            hasAppliedBehaviors = true;
        }

        //Initialize lifetime timer
        timeAlive = 0f;
    }

    //Removes the behaviors assigned in AddBehaviors()
    public override void RemoveBehaviors(Entity entity)
    {
        if (isPermanent)
        {
            //Debug.Log("Permanent behavior, not removing");
            return;
        }

        //Remove Behaviors
        //Debug.Log("Removing Behaviors");

        entity.onMoveCalculationEvents -= ChangeMovement;
        entity.customEvents -= CustomEvent;

        //Destroy component after removing behaviors since it is not permanent
        //Destroy(this);
    }

    private void ChangeMovement(Entity entity)
    {
        entity.nextPos += entity.transform.right * entity.speed * Time.fixedDeltaTime;
    }

    private void CustomEvent(Entity entity)
    {
        //Kill the entity if it has reached the end of its lifetime
        timeAlive += Time.fixedDeltaTime;
        if (timeAlive >= lifetime)
        {
            entity.Disable();
        }
    }
}
