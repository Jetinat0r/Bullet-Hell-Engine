using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    #region Manually Set Vars
    //The entity that this spawner fires. Can be overridden by spawner behaviors, but be careful
    public string spawnerEntityType;

    //The spawner behaviors that the spawner will always use
    [Tooltip("The spawner behaviors that the spawner will always use")]
    public List<string> defaultSpawnerBehaviors = new List<string>();

    //The entity behaviors that the spawner will always apply to the entities that it spawns
    [Tooltip("The entity behaviors that the spawner will always apply to the entities that it spawns")]
    public List<string> defaultEntityBehaviors = new List<string>();

    //The entity patterns that the spawner will always use when it spawns entities
    [Tooltip("The entity patterns that the spawner will always use when it spawns entities")]
    public List<string> defaultSpawnerPatterns = new List<string>();
    
    //Determines whether or not it should cycle through multiple spawnerPatterns, true if there is > 1 pattern in the list [UNUSED]
    //private bool cyclePatterns = false;
    //Holds what the last used pattern was
    private int curPatternIndex = 0;

    //Determines whether or not to stop firing once lastPattern == spawnerPatterns.Count
    //If true and randomizePatterns == false, it will cycle back to the first pattern
    [Tooltip("Determines whether or not to stop firing once lastPattern == spawnerPatterns.Count")]
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

    //The behaviors that the spawner applies to itself
    public List<SpawnerBehavior> spawnerBehaviors = new List<SpawnerBehavior>();

    //The behaviors to apply to every entity spawned via this spawner
    public List<EntityBehaviour> entityBehaviors = new List<EntityBehaviour>();

    //List of available patterns to shoot bullets out via
    [Tooltip("List of available patterns to shoot bullets out via")]
    public List<SpawnerPattern> spawnerPatterns = new List<SpawnerPattern>();
    #endregion

    #region Methods that EntityBehavior can Influence
    public delegate void UseSpawnerBehavior(EntitySpawner spawner);
    //updatePatternEvent is designed to notify an behavior when the pattern changes, but can be used for more nefarious practices as well
    //Recommended to check if totalPatternsUsed == 0 if your logic is for when it swaps between patterns
    public UseSpawnerBehavior onChangePatternEvents = null;
    //postSpawnPatternBurstEvents is used to impact how the spawner behaves after launching a burst from a pattern
    public UseSpawnerBehavior postSpawnPatternBurstEvents = null;
    //customEvents for when you want to get spicy w/ how the spawner behaves. Use at your own risk
    //Called before the spawner checks if it can fire this frame AND before it decrements its cooldown timer
    public UseSpawnerBehavior customEvents = null;

    public delegate void ModifyPatternData(EntitySpawner spawner, ref SpawnerPattern patternCopy);
    public ModifyPatternData hijackPatternDataEvents = null;

    public delegate void ModifyEntityInstantiation(EntitySpawner spawner, ref Vector3 position, ref Quaternion rotation, ref float entitySpeed, Entity entity);
    //onSpawnCalculationEvents allows for entities to be modified before they are "shot"
    public ModifyEntityInstantiation onSpawnCalculationEvents = null;
    #endregion

    #region Private Vars
    //Holds the pattern currently being used
    private SpawnerPattern curPattern;
    //Timer for cooldown between shots
    private float cooldownTimer = 0f;
    //Keeps track of how many times bursts are fired from the current pattern, compared to SpawnerPattern.numSpawns
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
    /// 1. Removes ALL behaviors and patterns from the spawner. It is highly recommended to ONLY use this when first creating a spawner
    /// 2. Adds default scriptable objects (spawner behaviors, entity behaviors, and entity patterns) if those bools are true
    /// 3. Adds custom scriptable objects (same types as last step) if it can
    /// 4. Enables all spawner behaviors
    /// 5. Resets the entity patterns
    /// </summary>
    public void Init(string _projType = null, bool _enableAfterInit = true, bool? _destroyOnDisable = null,
        List<SpawnerBehavior> _spawnerBehaviors = null, List<EntityBehaviour> _projBehaviors = null, List<SpawnerPattern> _projPatterns = null,
        bool _useDefaultSpawnerBehaviors = true, bool _useDefaultProjBehaviors = true, bool _useDefaultPatterns = true)
    {
        //Here to prevent the spawner from ever updating while being reset
        isEnabled = false;

        #region Remove Old Behaviors & Patterns
        RemoveSpawnerBehaviors(new List<SpawnerBehavior>(spawnerBehaviors));

        spawnerBehaviors.Clear();

        RemoveEntityBehaviors(new List<EntityBehaviour>(entityBehaviors));
        entityBehaviors.Clear();

        //TODO: Re-strucure SpawnerPatterns so that all spawners do is reference prefabs located in CachedBHEResources
        foreach(SpawnerPattern _pp in spawnerPatterns)
        {
            Destroy(_pp);
        }
        spawnerPatterns.Clear();
        #endregion

        #region Setup Default Behaviors
        //Sets up the default spawner behaviors
        //These get set to permanent as they will always be applied to their spawner (some exceptions apply, but you have to make those yourself)
        if (_useDefaultSpawnerBehaviors)
        {
            foreach (string _spawnerBehaviorType in defaultSpawnerBehaviors)
            {
                SpawnerBehavior _sBehavior = BehaviorManager.instance.GetSpawnerBehavior(_spawnerBehaviorType);
                if (_sBehavior != null)
                {
                    spawnerBehaviors.Add(_sBehavior);
                }
            }
        }


        //Sets up default entity behaviors
        if (_useDefaultProjBehaviors)
        {
            foreach (string _projBehaviorType in defaultEntityBehaviors)
            {
                EntityBehaviour _projBehavior = BehaviorManager.instance.GetEntityBehavior(_projBehaviorType);
                if (_projBehavior != null)
                {
                    entityBehaviors.Add(_projBehavior);
                }
            }
        }


        //Sets up default entity patterns
        if (_useDefaultPatterns)
        {
            foreach (string _projPatternType in defaultSpawnerPatterns)
            {
                SpawnerPattern _pattern = CachedBHEResources.instance.GetSpawnerPattern(_projPatternType);
                if (_pattern != null)
                {
                    spawnerPatterns.Add(_pattern);
                }
            }
        }
        #endregion

        #region Add Custom Behaviors
        if (_projType != null)
        {
            spawnerEntityType = _projType;
        }

        if(_projBehaviors != null)
        {
            AddEntityBehaviors(_projBehaviors);
        }

        if(_spawnerBehaviors != null)
        {
            AddSpawnerBehaviors(_spawnerBehaviors);
        }
        #endregion

        #region Enable Spawner Behaviors
        EnableSpawnerBehaviors();
        #endregion

        #region Reset Entity Patterns
        SetupPatterns(true);
        #endregion

        //Add late spawner behaviors
        foreach(SpawnerBehavior se in spawnerBehaviors)
        {
            se.LateAddBehaviors(this);
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

    #region Enable/Disable Spawner Behaviors
    private void EnableSpawnerBehaviors()
    {
        foreach(SpawnerBehavior _se in spawnerBehaviors)
        {
            _se.AddBehaviors(this);
        }
    }

    private void DisableSpawnerBehaviors()
    {
        foreach (SpawnerBehavior _se in spawnerBehaviors)
        {
            _se.RemoveBehaviors(this);
        }
    }
    #endregion

    #region Extra Methods to Non-Destructively Add and Remove Scriptable Objects from the Spawner
    public void AddSpawnerBehaviors(List<SpawnerBehavior> _spawnerBehaviors)
    {
        int countBefore = spawnerBehaviors.Count;

        foreach(SpawnerBehavior _se in _spawnerBehaviors)
        {
            SpawnerBehavior newSpawnerBehavior = BehaviorManager.instance.GetSpawnerBehavior(_se.SpawnerBehaviorName);
            spawnerBehaviors.Add(newSpawnerBehavior);

            newSpawnerBehavior.generationsToInheritBehavior = _se.generationsToInheritBehavior;

            newSpawnerBehavior.AddBehaviors(this);
        }

        //Do late add for the new behaviors
        for(int i = countBefore; i < spawnerBehaviors.Count; i++)
        {
            spawnerBehaviors[i].LateAddBehaviors(this);
        }

        //Update the old behaviors
        for(int i = 0; i < countBefore; i++)
        {
            spawnerBehaviors[i].UpdateBehaviors(this);
        }
    }

    //Intended as: nabbed from the spawner, then used to remove them
    //i.e. an Behavior grabs spawnerBehaviors, pulls out [0] and calls RemoveSpawnerBehaviors for that behavior
    public void RemoveSpawnerBehaviors(List<SpawnerBehavior> _spawnerBehaviors)
    {
        foreach (SpawnerBehavior _se in _spawnerBehaviors)
        {
            _se.RemoveBehaviors(this);
            spawnerBehaviors.Remove(_se);

            //Return behavior to pool
            BehaviorManager.instance.DeactivateSpawnerBehavior(_se);
        }
    }

    public void AddEntityBehaviors(List<EntityBehaviour> _entityBehaviors)
    {
        foreach (EntityBehaviour _pe in _entityBehaviors)
        {
            EntityBehaviour newEntityBehavior = BehaviorManager.instance.GetEntityBehavior(_pe.EntityBehaviorName);

            newEntityBehavior.generationsToInheritBehavior = _pe.generationsToInheritBehavior;

            entityBehaviors.Add(newEntityBehavior);
        }
    }

    public void RemoveEntityBehaviors(List<EntityBehaviour> _entityBehaviors)
    {
        foreach (EntityBehaviour _pe in _entityBehaviors)
        {
            entityBehaviors.Remove(_pe);
            
            //Return behavior to pool
            BehaviorManager.instance.DeactivateEntityBehavior(_pe);
        }
    }

    public void AddSpawnerPatterns(List<SpawnerPattern> _spawnerPatterns)
    {
        spawnerPatterns.AddRange(_spawnerPatterns);

        //Fix Pattern Place
        SetupPatterns(false);
    }

    public void RemoveSpawnerPatterns(List<SpawnerPattern> _spawnerPatterns)
    {
        foreach(SpawnerPattern _pp in _spawnerPatterns)
        {
            spawnerPatterns.Remove(_pp);
        }

        //Fix Pattern Place
        SetupPatterns(false);
    }
    #endregion

    //Applies spawner behaviors to the spawner
    public void Enable(bool resetPatterns = false)
    {
        EnableSpawnerBehaviors();

        SetupPatterns(resetPatterns);

        isEnabled = true;
    }

    //Disables spawner behaviors and resets it
    public void Disable()
    {
        isEnabled = false;

        DisableSpawnerBehaviors();

        if (destroyOnDisable)
        {
            //Returns behaviors to caches
            RemoveSpawnerBehaviors(new List<SpawnerBehavior>(spawnerBehaviors));
            RemoveEntityBehaviors(new List<EntityBehaviour>(entityBehaviors));

            Destroy(gameObject);
        }
    }

    //Sets up the pattern spawning related variables
    //If resetPatterns, resets the current position in the patterns to startup
    //If not resetPatterns, attempts to continue using the current pattern and moves on from there
    public void SetupPatterns(bool resetPatterns = false)
    {
        //Failure case, the spawner won't work w/o any patterns
        if (spawnerPatterns.Count == 0)
        {
            Debug.LogError($"No entity patterns found in EntitySpawner ({name})");
            return;
        }

        //If there is more than one pattern, it will cycle through all of the available patterns [UNUSED]
        //if(spawnerPatterns.Count > 1)
        //{
        //    //cyclePatterns = true;
        //}
        

        if (resetPatterns)
        {
            //Sets the first pattern to be used
            if (randomizePatterns)
            {
                if (spawnerPatterns.Count == 1)
                {
                    noRandomRepeats = false;
                    Debug.LogWarning($"noRandomRepeats turned off for ({name}) because there is only one pattern!");
                }
                curPatternIndex = Random.Range(0, spawnerPatterns.Count);
                curPattern = spawnerPatterns[curPatternIndex];
            }
            else
            {
                curPatternIndex = 0;
                curPattern = spawnerPatterns[0];
            }

            totalPatternsUsed = 0;
            onChangePatternEvents?.Invoke(this);
        }
        else
        {
            //Determines if the current pattern has been removed or not
            curPatternIndex = -1;
            for(int i = 0; i < spawnerPatterns.Count; i++)
            {
                if(curPattern == spawnerPatterns[i])
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
                if (spawnerPatterns.Count == 1)
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
            //Allows modders to be extra spicy w/ their spawner behaviors
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


        SpawnerPattern curPatternData = Instantiate(curPattern);
        hijackPatternDataEvents?.Invoke(this, ref curPatternData);

        //Here bc switch statements are weird
        Vector3 spawnerRotation;
        switch (curPatternData.spreadType)
        {
            //Spreads shots randomly between minAngle and maxAngle
            case SpawnerPattern.EntitySpreadType.RANDOM:
                for(int i = 0; i < curPatternData.numEntities; i++)
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
                    FireEntity(spawnerEntityType, projDirection, curPatternData.entitySpeed);
                }
                break;

            //Spreads shots evenly between minAngle and maxAngle
            case SpawnerPattern.EntitySpreadType.EVEN:
                //Determine the angle difference between shots. First entity always fires at minAngle
                float angleIncrement;
                if (curPatternData.numEntities == 1)
                {
                    //Here to avoid a divide by 0 error
                    angleIncrement = 0;
                }
                else
                {
                    angleIncrement = (curPatternData.maxAngle - curPatternData.minAngle) / (curPatternData.numEntities - 1);
                }

                //Store initial rotation to avoid clumping w/ rotating spawners
                spawnerRotation = transform.rotation.eulerAngles;
                //Fire entities
                for (int i = 0; i < curPatternData.numEntities; i++)
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
                    FireEntity(spawnerEntityType, projDirection, curPatternData.entitySpeed);
                }
                break;

            //Uses pre-determined angles from entityRotations
            case SpawnerPattern.EntitySpreadType.CUSTOM:
                //Store initial rotation to avoid clumping w/ rotating spawners
                spawnerRotation = transform.rotation.eulerAngles;
                for (int i = 0; i < curPatternData.numEntities; i++)
                {
                    Quaternion projDirection;
                    if (curPatternData.useSpawnerRotation)
                    {
                        projDirection = Quaternion.Euler(spawnerRotation + new Vector3(0f, 0f, curPatternData.entityRotations[i]));
                    }
                    else
                    {
                        projDirection = Quaternion.Euler(new Vector3(0f, 0f, curPatternData.entityRotations[i]));
                    }
                    FireEntity(spawnerEntityType, projDirection, curPatternData.entitySpeed);
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
                for(int i = 0; i < spawnerPatterns.Count; i++)
                {
                    exclusiveList.Add(i);
                }
                exclusiveList.Remove(curPatternIndex);

                curPatternIndex = exclusiveList[Random.Range(0, exclusiveList.Count)];
            }
            else
            {
                curPatternIndex = Random.Range(0, spawnerPatterns.Count);
            }

            curPattern = spawnerPatterns[curPatternIndex];
            timesSpawned = 0;
            //totalPatternsUsed++;
            onChangePatternEvents?.Invoke(this);
            return true;
        }
        else
        {
            //Increments the pattern from the list, looping if allowed to. If the incrementation succeeds, returns true, else returns false
            curPatternIndex += 1;
            if(curPatternIndex == spawnerPatterns.Count)
            {
                if (fireIndefinitely || loopPatterns)
                {
                    curPatternIndex = 0;
                    curPattern = spawnerPatterns[0];
                }
                else
                {
                    return false;
                }
            }
            else
            {
                curPattern = spawnerPatterns[curPatternIndex];
            }

            timesSpawned = 0;
            //totalPatternsUsed++;
            onChangePatternEvents?.Invoke(this);
            return true;
        }
    }

    private void FireEntity(string _spawnerEntityType, Quaternion _rotation, float _entitySpeed)
    {
        //Gets a entity component attatched to a GameObject
        Entity entity = EntityManager.instance.GetEntity(_spawnerEntityType);

        //Creates mutable variables for spawner behaviors to mess with
        Vector3 _position = transform.position;
        float _damage = 0f;

        //Enables spawner behaviors to mess with the spawning of entities
        onSpawnCalculationEvents?.Invoke(this, ref _position, ref _rotation, ref _entitySpeed, entity);

        //Sets up the position and rotation of entities in a way that avoids obvious snapping
        entity.transform.SetPositionAndRotation(_position, _rotation);

        //Apply behaviors attatched to the spawner to the entity
        entity.HijackEntity(entityBehaviors, _damage, _entitySpeed);

        //Shoot the entity
        entity.Initialize();
    }
}
