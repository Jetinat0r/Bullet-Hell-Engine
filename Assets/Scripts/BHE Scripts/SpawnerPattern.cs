using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Entity Pattern", menuName = "ScriptableObjects/Entity Pattern")]
public class SpawnerPattern : ScriptableObject
{
    public enum EntitySpreadType
    {
        RANDOM,
        EVEN,
        CUSTOM,
    }

    public string patternType;

    //If true, aligns pattern to spawner rotation, else aligns pattern to world space
    public bool useSpawnerRotation;

    //The minimum and maximum angle for which the shots are spread through
    public float minAngle;
    public float maxAngle;

    //Whether the spread of the entities should be random, evenly space, or customly spaced
    public EntitySpreadType spreadType;

    //Number of entities fired in one burst
    [Tooltip("Number of entities fired in one burst")]
    public int numEntities;

    //If spreadType == CUSTOM, use these for rotations
    //Length MUST equal numEntities
    [Tooltip("Used when spreadType == CUSTOM, used for rotations. Length MUST equal numEntities")]
    public float[] entityRotations;

    //Number of times the pattern is produced
    [Tooltip("Number of times the pattern is produced")]
    public int numSpawns;

    //The speed at which entities are initially fired at
    public float entitySpeed;

    //The time between shot spawns (only used if numSpawns > 1)
    public float shotCooldown;
}