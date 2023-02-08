using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sine Wave Behavior", menuName = "ScriptableObjects/Entity Behaviors/Sine Wave Entity Behavior")]
public class SineWaveEntityBehavior : EntityBehaviour
{
    public new const string EntityBehaviorType = "Sine_Wave_Entity_Behavior";
    public override string GetEntityBehaviorType() => EntityBehaviorType;

    [Space(10f)]
    [Header("Sine Wave Vars")]
    //How often waves appear
    public float frequency = 1f;
    //Max distance from the sin wave's "center"
    public float magnitude = 0.5f;
    //Where the entity is on the curve
    private float sinPos = 0f;
    //How far from "normal" the wave is
    private float lastPosition;

    //Used to reset local values on copy
    public override void Init()
    {
        base.Init();
        sinPos = 0f;
        lastPosition = 0f;
    }

    //Assigns behaviors to the correct places in Entity's sequence of events
    public override void AddBehaviors(Entity entity)
    {
        if (!hasAppliedBehaviors)
        {
            //Apply Behaviors
            //Debug.Log("Applying Behaviors!");

            entity.onMoveCalculationEvents += ChangeMovement;

            hasAppliedBehaviors = true;
        }

        //Initialize sin position and lastOffset
        sinPos = 0f;
        lastPosition = 0f;
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
        //Debug.Log("RemovingBehaviors");

        entity.onMoveCalculationEvents -= ChangeMovement;

        //Destroy component after removing behaviors since it is not permanent
        //Destroy(this);
    }

    private void ChangeMovement(Entity entity)
    {
        float newPosition = Mathf.Sin(sinPos * frequency) * Time.fixedDeltaTime;

        //Ends up moving the pos up or down by the difference between the old position and the new position to create the offset to follow the sine wave
        entity.nextPos += entity.transform.up * (newPosition - lastPosition) * magnitude;

        lastPosition = newPosition;
        sinPos += Time.fixedDeltaTime;
    }
}
