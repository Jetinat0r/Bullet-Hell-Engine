using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileSpawner : MonoBehaviour
{
    #region Manually Set Vars
    //The projectile that this spawner fires. Can be overridden by spawner effects, but be careful
    public string spawnerProjectileType;

    //The spawner effects that the spawner will always use
    [Tooltip("The spawner effects that the spawner will always use")]
    public List<string> defaultSpawnerEffects = new List<string>();

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

    //Determines if the spawner should stop shooting after running through each of its patterns. Only used if randomizePatterns and fireIndefinitely are false
    [Tooltip("Determines if the spawner should loop through its patterns after after exhausting them. Only used if randomizePatterns and fireIndefinitely are false")]
    public bool loopPatterns = true;

    //Decides if the the randomized patterns should be able to use the same pattern twice in a row or not. Only used if randomizePatterns is true
    [Tooltip("Decides if the the randomized patterns should be able to use the same pattern twice in a row or not. Only used if randomizePatterns is true")]
    public bool noRandomRepeats = false;

    //Whether or not the spawner should destroy it's gameobject upon disable, usually for destroying when the pattern completes
    public bool destroyOnDisable = false;
    #endregion

    #region Dynamically Set Vars
    //If the spawner is firing
    public bool isEnabled = false;

    //The effects that the spawner applies to itself
    public List<SpawnerEffect> spawnerEffects = new List<SpawnerEffect>();

    //The effects to apply to every projectile spawned via this spawner
    public List<ProjectileEffect> projectileEffects = new List<ProjectileEffect>();

    //List of available patterns to shoot bullets out via
    [Tooltip("List of available patterns to shoot bullets out via")]
    public List<ProjectilePattern> projectilePatterns = new List<ProjectilePattern>();
    #endregion

    #region Methods that ProjectileEffect can Influence
    public delegate void UseSpawnerEffect(ProjectileSpawner spawner);
    //updatePatternEvent is designed to notify an effect when the pattern changes, but can be used for more nefarious practices as well
    //Recommended to check if totalPatternsUsed == 0 if your logic is for when it swaps between patterns
    public UseSpawnerEffect onChangePatternEvents = null;
    //postSpawnPatternBurstEvents is used to impact how the spawner behaves after launching a burst from a pattern
    public UseSpawnerEffect postSpawnPatternBurstEvents = null;
    //customEvents for when you want to get spicy w/ how the spawner behaves. Use at your own risk
    //Called before the spawner checks if it can fire this frame AND before it decrements its cooldown timer
    public UseSpawnerEffect customEvents = null;

    public delegate void ModifyPatternData(ProjectileSpawner spawner, ref ProjectilePattern patternCopy);
    public ModifyPatternData hijackPatternDataEvents = null;

    public delegate void ModifyProjectileInstantiation(ProjectileSpawner spawner, ref Vector3 position, ref Quaternion rotation, ref float projectileSpeed, Projectile projectile);
    //onSpawnCalculationEvents allows for projectiles to be modified before they are "shot"
    public ModifyProjectileInstantiation onSpawnCalculationEvents = null;
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

    //TODO: Remove TestVar
    public bool TESTVAR = false;
    private void Start()
    {
        if (TESTVAR)
        {
            Init();

        }
    }

    /// <summary>
    /// Allows another object to override properties of the spawner
    /// Steps:
    /// 1. Removes ALL effects and patterns from the spawner. It is highly recommended to ONLY use this when first creating a spawner
    /// 2. Adds default scriptable objects (spawner effects, projectile effects, and projectile patterns) if those bools are true
    /// 3. Adds custom scriptable objects (same types as last step) if it can
    /// 4. Enables all spawner effects
    /// 5. Resets the projectile patterns
    /// </summary>
    public void Init(string _projType = null, bool _enableAfterInit = true, bool? _destroyOnDisable = null,
        List<SpawnerEffect> _spawnerEffects = null, List<ProjectileEffect> _projEffects = null, List<ProjectilePattern> _projPatterns = null,
        bool _useDefaultSpawnerEffects = true, bool _useDefaultProjEffects = true, bool _useDefaultPatterns = true)
    {
        //Here to prevent the spawner from ever updating while being reset
        isEnabled = false;

        //TODO: RE-add to cache
        #region Remove Old Effects & Patterns
        foreach (SpawnerEffect _se in spawnerEffects)
        {
            _se.RemoveEffects(this);
            Destroy(_se);
        }
        spawnerEffects.Clear();

        foreach(ProjectileEffect _pe in projectileEffects)
        {
            Destroy(_pe);
        }
        projectileEffects.Clear();

        foreach(ProjectilePattern _pp in projectilePatterns)
        {
            Destroy(_pp);
        }
        projectilePatterns.Clear();
        #endregion

        #region Setup Default Effects
        //Sets up the default spawner effects
        //These get set to permanent as they will always be applied to their spawner (some exceptions apply, but you have to make those yourself)
        if (_useDefaultSpawnerEffects)
        {
            foreach (string _spawnerEffectType in defaultSpawnerEffects)
            {
                SpawnerEffect _sEffect = CachedBHEResources.instance.GetSpawnerEffect(_spawnerEffectType);
                if (_sEffect != null)
                {
                    spawnerEffects.Add(_sEffect);
                }
            }
        }


        //Sets up default projectile effects
        if (_useDefaultProjEffects)
        {
            foreach (string _projEffectType in defaultProjectileEffects)
            {
                ProjectileEffect _projEffect = CachedBHEResources.instance.GetProjectileEffect(_projEffectType);
                if (_projEffect != null)
                {
                    projectileEffects.Add(_projEffect);
                }
            }
        }


        //Sets up default projectile patterns
        if (_useDefaultPatterns)
        {
            foreach (string _projPatternType in defaultProjectilePatterns)
            {
                ProjectilePattern _pattern = CachedBHEResources.instance.GetProjectilePattern(_projPatternType);
                if (_pattern != null)
                {
                    projectilePatterns.Add(_pattern);
                }
            }
        }
        #endregion

        #region Add Custom Effects
        if (_projType != null)
        {
            spawnerProjectileType = _projType;
        }

        if(_projEffects != null)
        {
            AddProjectileEffects(_projEffects);
        }

        if(_spawnerEffects != null)
        {
            AddSpawnerEffects(_spawnerEffects);
        }
        #endregion

        #region Enable Spawner Effects
        EnableSpawnerEffects();
        #endregion

        #region Reset Projectile Patterns
        SetupPatterns(true);
        #endregion

        //Add late spawner effects
        foreach(SpawnerEffect se in spawnerEffects)
        {
            se.LateAddEffects(this);
        }

        //For if someone wants to conveniently change destroyOnEnable
        if(_destroyOnDisable != null)
        {
            destroyOnDisable = (bool)_destroyOnDisable;
        }

        if (_enableAfterInit)
        {
            //Allows the spawner to start working again
            isEnabled = true;
        }
    }

    #region Enable/Disable Spawner Effects
    private void EnableSpawnerEffects()
    {
        foreach(SpawnerEffect _se in spawnerEffects)
        {
            _se.AddEffects(this);
        }
    }

    private void DisableSpawnerEffects()
    {
        foreach (SpawnerEffect _se in spawnerEffects)
        {
            _se.RemoveEffects(this);
        }
    }
    #endregion

    #region Extra Methods to Non-Destructively Add and Remove Scriptable Objects from the Spawner
    public void AddSpawnerEffects(List<SpawnerEffect> _spawnerEffects)
    {
        int countBefore = spawnerEffects.Count;

        foreach(SpawnerEffect _se in _spawnerEffects)
        {
            SpawnerEffect newSpawnerEffect = CachedBHEResources.instance.GetSpawnerEffect(_se.spawnerEffectName);
            spawnerEffects.Add(newSpawnerEffect);

            newSpawnerEffect.generationsToInheritEffect = _se.generationsToInheritEffect;

            newSpawnerEffect.AddEffects(this);
        }

        //Do late add for the new effects
        for(int i = countBefore; i < spawnerEffects.Count; i++)
        {
            spawnerEffects[i].LateAddEffects(this);
        }

        //Update the old effects
        for(int i = 0; i < countBefore; i++)
        {
            spawnerEffects[i].UpdateEffects(this);
        }
    }

    //Intended as: nabbed from the spawner, then used to remove them
    //i.e. an Effect grabs spawnerEffects, pulls out [0] and calls RemoveSpawnerEffects for that effect
    public void RemoveSpawnerEffects(List<SpawnerEffect> _spawnerEffects)
    {
        foreach (SpawnerEffect _se in _spawnerEffects)
        {
            _se.RemoveEffects(this);
            spawnerEffects.Remove(_se);
            //TODO: Add back to pool
        }
    }

    public void AddProjectileEffects(List<ProjectileEffect> _projectileEffects)
    {
        foreach (ProjectileEffect _pe in _projectileEffects)
        {
            ProjectileEffect newProjectileEffect = CachedBHEResources.instance.GetProjectileEffect(_pe.projectileEffectName);

            newProjectileEffect.generationsToInheritEffect = _pe.generationsToInheritEffect;

            projectileEffects.Add(newProjectileEffect);
        }
    }

    public void RemoveProjectileEffects(List<ProjectileEffect> _projectileEffects)
    {
        foreach (ProjectileEffect _pe in _projectileEffects)
        {
            projectileEffects.Remove(_pe);
            //TODO: Add back to pool
        }
    }

    public void AddProjectilePatterns(List<ProjectilePattern> _projectilePatterns)
    {
        projectilePatterns.AddRange(_projectilePatterns);

        //Fix Pattern Place
        SetupPatterns(false);
    }

    public void RemoveProjectilePatterns(List<ProjectilePattern> _projectilePatterns)
    {
        foreach(ProjectilePattern _pp in _projectilePatterns)
        {
            projectilePatterns.Remove(_pp);
        }

        //Fix Pattern Place
        SetupPatterns(false);
    }
    #endregion

    //Applies spawner effects to the spawner
    public void Enable(bool resetPatterns = false)
    {
        EnableSpawnerEffects();

        SetupPatterns(resetPatterns);

        isEnabled = true;
    }

    //Disables spawner effects and resets it
    public void Disable()
    {
        isEnabled = false;

        DisableSpawnerEffects();

        if (destroyOnDisable)
        {
            //TODO: Return stuff to caches
            Destroy(gameObject);
        }
    }

    //Sets up the pattern spawning related variables
    //If resetPatterns, resets the current position in the patterns to startup
    //If not resetPatterns, attempts to continue using the current pattern and moves on from there
    public void SetupPatterns(bool resetPatterns = false)
    {
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
        

        if (resetPatterns)
        {
            //Sets the first pattern to be used
            if (randomizePatterns)
            {
                if (projectilePatterns.Count == 1)
                {
                    noRandomRepeats = false;
                    Debug.LogWarning($"noRandomRepeats turned off for ({name}) because there is only one pattern!");
                }
                curPatternIndex = Random.Range(0, projectilePatterns.Count);
                curPattern = projectilePatterns[curPatternIndex];
            }
            else
            {
                curPatternIndex = 0;
                curPattern = projectilePatterns[0];
            }

            totalPatternsUsed = 0;
            onChangePatternEvents?.Invoke(this);
        }
        else
        {
            //Determines if the current pattern has been removed or not
            curPatternIndex = -1;
            for(int i = 0; i < projectilePatterns.Count; i++)
            {
                if(curPattern == projectilePatterns[i])
                {
                    curPatternIndex = i;
                }
            }

            //If the current pattern was removed, reset the patterns, else continue checks
            if(curPatternIndex == -1)
            {
                SetupPatterns(true);
            }

            if (randomizePatterns)
            {
                if (projectilePatterns.Count == 1)
                {
                    noRandomRepeats = false;
                    Debug.LogWarning($"noRandomRepeats turned off for ({name}) because there is only one pattern!");
                }
            }
        }

        if (patternUses == 0)
        {
            Debug.LogWarning($"Pattern uses is set to 0 for ({name}), setting fireIndefinitely to true!");
            fireIndefinitely = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnabled)
        {
            //Allows modders to be extra spicy w/ their spawner effects
            customEvents?.Invoke(this);

            //Decrement cooldown timer frame independently. ALWAYS FIRST
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                FireCurPatternBurst();

                postSpawnPatternBurstEvents?.Invoke(this);

                if (IsCurPatternCompleted())
                {
                    if (!AttemptSwapPattern())
                    {
                        //If the spawner runs out of patterns it's allowed to use it disables itself
                        Disable();
                    }
                }
            }
        }
    }

    //Fires one "Burst" from the current pattern
    private void FireCurPatternBurst()
    {
        //I might have some form of "failed to shoot" which I'd put either here or above, which could skip some things


        ProjectilePattern curPatternData = Instantiate(curPattern);
        hijackPatternDataEvents?.Invoke(this, ref curPatternData);

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
        return timesSpawned >= curPattern.numSpawns;
    }

    //If the pattern can be swapped, do so and return true, else return false
    private bool AttemptSwapPattern()
    {
        totalPatternsUsed++;

        if(!fireIndefinitely && totalPatternsUsed >= patternUses)
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
            //totalPatternsUsed++;
            onChangePatternEvents?.Invoke(this);
            return true;
        }
        else
        {
            //Increments the pattern from the list, looping if allowed to. If the incrementation succeeds, returns true, else returns false
            curPatternIndex += 1;
            if(curPatternIndex == projectilePatterns.Count)
            {
                if (fireIndefinitely || loopPatterns)
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
            //totalPatternsUsed++;
            onChangePatternEvents?.Invoke(this);
            return true;
        }
    }

    private void FireProjectile(string _spawnerProjectileType, Quaternion _rotation, float _projectileSpeed)
    {
        //Gets a projectile component attatched to a GameObject
        Projectile projectile = ProjectileManager.instance.GetProjectile(_spawnerProjectileType);

        //Creates mutable variables for spawner effects to mess with
        Vector3 _position = transform.position;
        float _damage = 0f;

        //Enables spawner effects to mess with the spawning of projectiles
        onSpawnCalculationEvents?.Invoke(this, ref _position, ref _rotation, ref _projectileSpeed, projectile);

        //Sets up the position and rotation of projectiles in a way that avoids obvious snapping
        projectile.transform.SetPositionAndRotation(_position, _rotation);

        //Apply effects attatched to the spawner to the projectile
        projectile.HijackProjectile(projectileEffects, _damage, _projectileSpeed);

        //Shoot the projectile
        projectile.Initialize();
    }
}
