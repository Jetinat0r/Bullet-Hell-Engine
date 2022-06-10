using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Projectile Pattern", menuName = "ScriptableObjects/Projectile Pattern")]
public class ProjectilePattern : ScriptableObject
{
    public enum ProjectileSpreadType
    {
        RANDOM,
        EVEN,
        CUSTOM,
    }

    //Holds data, I'm gonna have to figure this out
    public Projectile projectilePrefab;

    //If true, aligns pattern to spawner rotation, else aligns pattern to world space
    public bool useSpawnerRotation;

    //The minimum and maximum angle for which the shots are spread through
    public float minAngle;
    public float maxAngle;

    //Whether the spread of the projectiles should be random, evenly space, or customly spaced
    public ProjectileSpreadType spreadType;

    //Number of projectiles fired in one burst
    [Tooltip("Number of projectiles fired in one burst")]
    public int numProjectiles;

    //If spreadType == CUSTOM, use these for rotations
    //Length MUST equal numProjectiles
    [Tooltip("Used when spreadType == CUSTOM, used for rotations. Length MUST equal numProjectiles")]
    public float[] projectileRotations;

    //Number of times the pattern is produced
    [Tooltip("Number of times the pattern is produced")]
    public int numSpawns;

    //The speed at which projectiles are initially fired at
    public float projectileSpeed;

    //The time between shot spawns (only used if numSpawns > 1)
    public float shotCooldown;
}