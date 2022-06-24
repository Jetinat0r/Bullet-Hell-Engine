using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Rotation Spawner Effect", menuName = "ScriptableObjects/Spawner Effects/Rotation Spawner Effect")]
public class RotateSpawnerEffect :SpawnerEffect
{
    [Space(10)]
    [Header("Rotate Vars")]
    public float rotationSpeed = 1f;

    public override void Init()
    {
        hasAppliedEffects = false;
    }

    public override void AddEffects(ProjectileSpawner spawner)
    {
        if (!hasAppliedEffects)
        {
            spawner.customEvents += RotateSpawner;
        }

        hasAppliedEffects = true;
    }

    public override void RemoveEffects(ProjectileSpawner spawner)
    {
        hasAppliedEffects = false;
    }

    private void RotateSpawner(ProjectileSpawner spawner)
    {
        spawner.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }
}
