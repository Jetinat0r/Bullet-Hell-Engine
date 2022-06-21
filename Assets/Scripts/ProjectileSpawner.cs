using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    #region Manually Set Vars
    //The projectile that this spawner fires. Can be overridden by spawner effects, but be careful
    public string spawnerProjectileType;

    //The projectile effects that the spawner will always apply to the projectiles that it spawns
    [Tooltip("The projectile effects that the spawner will always apply to the projectiles that it spawns")]
    public List<string> defaultProjectileEffects = new List<string>();

    //The projectile patterns that the spawner will always use when it spawns projectiles
    [Tooltip("The projectile patterns that the spawner will always use when it spawns projectiles")]
    public List<string> defaultProjectilePatterns = new List<string>();
    
    //Determines whether or not it should cycle through multiple projectilePatterns, true if there is > 1 pattern in the list [UNUSED]
    //private bool cyclePatterns = false;
    //Holds what the last used pattern was
    private int curPatternIndex = 0;

    //Determines whether or not to stop firing once lastPattern == projectilePatterns.Count
    //If true and randomizePatterns == false, it will cycle back to the first pattern
    [Tooltip("Determines whether or not to stop firing once lastPattern == projectilePatterns.Count")]
    public bool fireIndefinitely = false;

    //Determines how many patterns to fire from the given list
    //Intended for random patterns, but can be used without randomization
    [Tooltip("Determines how many patterns to fire from the given list")]
    public int patternUses = 0;

    //Decides if the pojectile pattern used should be random or not
    [Tooltip("Decides if the pojectile pattern used should be random or not")]
    public bool randomizePatterns = false;

    //Determines if the spawner should stop shooting after running through each of its patterns. Only used if randomizePatterns is false
    [Tooltip("Determines if the spawner should stop shooting after running through each of its patterns. Only used if randomizePatterns is false")]
    public bool stopOnCompletion = true;

    //Decides if the the randomized patterns should be able to use the same pattern twice in a row or not. Only used if randomizePatterns is true
    [Tooltip("Decides if the the randomized patterns should be able to use the same pattern twice in a row or not. Only used if randomizePatterns is true")]
    public bool noRandomRepeats = false;
    #endregion

    #region Dynamically Set Vars
    //The effects to apply to every projectile spawned via this spawner
    public List<ProjectileEffect> projectileEffects = new List<ProjectileEffect>();

    //List of available patterns to shoot bullets out via
    [Tooltip("List of available patterns to shoot bullets out via")]
    public List<ProjectilePattern> projectilePatterns = new List<ProjectilePattern>();
    #endregion

    #region Private Vars
    //Holds the pattern currently being used
    private ProjectilePattern curPattern;
    //Timer for cooldown between shots
    private float cooldownTimer = 0f;
    //Keeps track of how many times bursts are fired from the current pattern, compared to ProjectilePattern.numSpawns
    private int timesSpawned = 0;
    //Keeps track of how many patterns have been started, in practice used to stop random patterns
    private int totalPatternsUsed = 0;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //Sets up default projectile effects
        foreach (string _projEffectType in defaultProjectileEffects)
        {
            ProjectileEffect _effect = CachedBHEResources.instance.GetProjectileEffect(_projEffectType);
            if (_effect != null)
            {
                projectileEffects.Add(_effect);
            }
        }

        //Sets up default projectile patterns
        foreach (string _projPatternType in defaultProjectilePatterns)
        {
            ProjectilePattern _pattern = CachedBHEResources.instance.GetProjectilePattern(_projPatternType);
            if (_pattern != null)
            {
                projectilePatterns.Add(_pattern);
            }
        }

        //Failure case, the spawner won't work w/o any patterns
        if (projectilePatterns.Count == 0)
        {
            Debug.LogError($"No projectile patterns found in ProjectileSpawner ({name})");
            return;
        }

        //If there is more than one pattern, it will cycle through all of the available patterns [UNUSED]
        //if(projectilePatterns.Count > 1)
        //{
        //    //cyclePatterns = true;
        //}
        curPatternIndex = 0;

        //Sets the first pattern to be used
        if (randomizePatterns)
        {
            if(projectilePatterns.Count == 1)
            {
                noRandomRepeats = false;
                Debug.LogWarning($"noRandomRepeats turned off for ({name}) because there is only one pattern!");
            }
            curPattern = projectilePatterns[Random.Range(0, projectilePatterns.Count)];
        }
        else
        {
            curPattern = projectilePatterns[0];
        }

        if(patternUses == 0)
        {
            Debug.LogWarning($"Pattern uses is set to 0 for ({name}), setting fireIndefinitely to true!");
            fireIndefinitely = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Decrement cooldown timer frame independently. ALWAYS FIRST
        cooldownTimer -= Time.deltaTime;

        //TODO: Fire bullets via available rules 
        if(cooldownTimer <= 0f)
        {
            FireCurPattern();

            if (IsCurPatternCompleted())
            {
                //if ((cyclePatterns && !AttemptSwapPattern()) || !cyclePatterns)
                //{
                //    //TODO: Destroy? spawner
                //    Destroy(gameObject);
                //}
                if (!AttemptSwapPattern())
                {
                    //TODO: Something about stop on completion? idk yet
                    Destroy(gameObject);
                }
            }
        }
    }

    private void FireCurPattern()
    {
        //I might have some form of "failed to shoot" which I'd put either here or above, which could skip some things


        ProjectilePattern curPatternData = Instantiate(curPattern);
        //TODO: HijackCurPatternData()

        //Here bc switch statements are weird
        Vector3 spawnerRotation;
        switch (curPatternData.spreadType)
        {
            //Spreads shots randomly between minAngle and maxAngle
            case ProjectilePattern.ProjectileSpreadType.RANDOM:
                for(int i = 0; i < curPatternData.numProjectiles; i++)
                {
                    Quaternion projDirection;
                    if (curPatternData.useSpawnerRotation)
                    {
                        projDirection = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, 0f, Random.Range(curPatternData.minAngle, curPatternData.maxAngle)));
                    }
                    else
                    {
                        projDirection = Quaternion.Euler(new Vector3(0f, 0f, Random.Range(curPatternData.minAngle, curPatternData.maxAngle)));
                    }
                    FireProjectile(spawnerProjectileType, projDirection, curPatternData.projectileSpeed);
                }
                break;

            //Spreads shots evenly between minAngle and maxAngle
            case ProjectilePattern.ProjectileSpreadType.EVEN:
                //Determine the angle difference between shots. First projectile always fires at minAngle
                float angleIncrement;
                if (curPatternData.numProjectiles == 1)
                {
                    //Here to avoid a divide by 0 error
                    angleIncrement = 0;
                }
                else
                {
                    angleIncrement = (curPatternData.maxAngle - curPatternData.minAngle) / (curPatternData.numProjectiles - 1);
                }

                //Store initial rotation to avoid clumping w/ rotating spawners
                spawnerRotation = transform.rotation.eulerAngles;
                //Fire projectiles
                for (int i = 0; i < curPatternData.numProjectiles; i++)
                {
                    Quaternion projDirection;
                    if (curPatternData.useSpawnerRotation)
                    {
                        projDirection = Quaternion.Euler(spawnerRotation + new Vector3(0f, 0f, curPatternData.minAngle + (angleIncrement * i)));
                    }
                    else
                    {
                        projDirection = Quaternion.Euler(new Vector3(0f, 0f, curPatternData.minAngle + (angleIncrement * i)));
                    }
                    FireProjectile(spawnerProjectileType, projDirection, curPatternData.projectileSpeed);
                }
                break;

            //Uses pre-determined angles from projectileRotations
            case ProjectilePattern.ProjectileSpreadType.CUSTOM:
                //Store initial rotation to avoid clumping w/ rotating spawners
                spawnerRotation = transform.rotation.eulerAngles;
                for (int i = 0; i < curPatternData.numProjectiles; i++)
                {
                    Quaternion projDirection;
                    if (curPatternData.useSpawnerRotation)
                    {
                        projDirection = Quaternion.Euler(spawnerRotation + new Vector3(0f, 0f, curPatternData.projectileRotations[i]));
                    }
                    else
                    {
                        projDirection = Quaternion.Euler(new Vector3(0f, 0f, curPatternData.projectileRotations[i]));
                    }
                    FireProjectile(spawnerProjectileType, projDirection, curPatternData.projectileSpeed);
                }
                break;
        }


        //Push the cooldown timer up. Even if the pattern changes after this shot, it should be forced to wait so the player has time to react to the old pattern before moving on
        cooldownTimer = curPatternData.shotCooldown;
        timesSpawned++;
    }

    private bool IsCurPatternCompleted()
    {
        return timesSpawned == curPattern.numSpawns;
    }

    //If the pattern can be swapped, do so and return true, else return false
    private bool AttemptSwapPattern()
    {
        //Debug.Log($"Using pattern {curPattern.name}");
        if(!fireIndefinitely && totalPatternsUsed == patternUses)
        {
            return false;
        }

        if (randomizePatterns)
        {
            //Swaps the pattern to a random one from the list
            if (noRandomRepeats)
            {
                //Generates a list to randomly choose from that excludes the previously used pattern
                List<int> exclusiveList = new List<int>();
                for(int i = 0; i < projectilePatterns.Count; i++)
                {
                    exclusiveList.Add(i);
                }
                exclusiveList.Remove(curPatternIndex);

                curPatternIndex = exclusiveList[Random.Range(0, exclusiveList.Count)];
            }
            else
            {
                curPatternIndex = Random.Range(0, projectilePatterns.Count);
            }

            curPattern = projectilePatterns[curPatternIndex];
            timesSpawned = 0;
            totalPatternsUsed++;
            return true;
        }
        else
        {
            //Increments the pattern from the list, looping if allowed to. If the incrementation succeeds, returns true, else returns false
            curPatternIndex += 1;
            if(curPatternIndex == projectilePatterns.Count)
            {
                if (fireIndefinitely)
                {
                    curPatternIndex = 0;
                    curPattern = projectilePatterns[0];
                }
                else
                {
                    return false;
                }
            }
            else
            {
                curPattern = projectilePatterns[curPatternIndex];
            }

            timesSpawned = 0;
            totalPatternsUsed++;
            return true;
        }
    }

    private void FireProjectile(string _spawnerProjectileType, Quaternion _rotation, float _projectileSpeed)
    {
        //Gets a projectile component attatched to a GameObject
        Projectile projectile = ProjectileManager.instance.GetProjectile(_spawnerProjectileType);

        //Sets up the position and rotation of projectiles in a way that avoids obvious snapping
        projectile.transform.SetPositionAndRotation(transform.position, _rotation);

        //Apply effects attatched to the spawner to the projectile
        projectile.HijackProjectile(projectileEffects, 0f, 2f);

        //Shoot the projectile
        projectile.Initialize();
    }
}
