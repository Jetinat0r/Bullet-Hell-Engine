using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Rotation Spawner Behavior", menuName = "ScriptableObjects/Spawner Behaviors/Rotation Spawner Behavior")]
public class RotateSpawnerBehavior : SpawnerBehavior
{
    public new const string SpawnerBehaviorType = "Rotate_Spawner";
    public override string GetSpawnerBehaviorType() => SpawnerBehaviorType;

    [Space(10)]
    [Header("Rotate Vars")]
    public float rotationSpeed = 1f;

    public override void Init()
    {
        hasAppliedBehaviors = false;
    }

    public override void AddBehaviors(EntitySpawner spawner)
    {
        if (!hasAppliedBehaviors)
        {
            spawner.customEvents += RotateSpawner;
        }

        hasAppliedBehaviors = true;
    }

    public override void RemoveBehaviors(EntitySpawner spawner)
    {
        spawner.customEvents -= RotateSpawner;

        hasAppliedBehaviors = false;
    }

    private void RotateSpawner(EntitySpawner spawner)
    {
        spawner.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
