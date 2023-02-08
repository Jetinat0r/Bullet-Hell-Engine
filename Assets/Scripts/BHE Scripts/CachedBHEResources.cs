using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColliderType
{
    CIRCLE = 0,
    BOX = 1,
    CAPSULE = 2,
    EDGE = 3,
    POLYGON = 4,
}

public class CachedBHEResources : MonoBehaviour
{
    public static CachedBHEResources instance;

    //The base entity from which every other entity will generate its prefab from
    public Entity baseEntity;
    //Stores all base entity prefabs, the key is the entityType
    public Dictionary<string, Entity> entityPrefabs = new Dictionary<string, Entity>();

    //List for me to add spawner behaviors to the dictionary
    [SerializeField]
    [Tooltip("These are for setting up spawner behaviors via the editor")]
    private List<SpawnerBehavior> editorSpawnerBehaviorScripObjs = new List<SpawnerBehavior>();
    //Every available Entity Behavior that spawners can choose from. The behaviors can still be modified after being copied over, but these are the defaults
    public Dictionary<string, SpawnerBehavior> spawnerBehaviors = new Dictionary<string, SpawnerBehavior>();

    //List for me to add entity behaviors to the dictionary
    [SerializeField] [Tooltip("These are for setting up entity behaviors via the editor")]
    private List<EntityBehaviour> editorEntityBehaviorScripObjs = new List<EntityBehaviour>();
    //Every available Entity Behavior that spawners can choose from. The behaviors can still be modified after being copied over, but these are the defaults
    public Dictionary<string, EntityBehaviour> entityBehaviors = new Dictionary<string, EntityBehaviour>();

    //List for me to add entity patterns to the dictionary
    [SerializeField] [Tooltip("These are for setting up entity behaviors via the editor")]
    private List<SpawnerPattern> editorSpawnerPatternScripObjs = new List<SpawnerPattern>();
    //Every available Entity Behavior that spawners can choose from. The behaviors can still be modified after being copied over, but these are the defaults
    public Dictionary<string, SpawnerPattern> spawnerPatterns = new Dictionary<string, SpawnerPattern>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadEditorPrefabs();
        
        ModManager.instance.LoadMods();

        //THIS IS ONLY FOR MOD TESTING, SHOULD BE CHANGED FOR FULL ENGINE RELEASE
        ModTester.instance.Init();
    }

    private void LoadEditorPrefabs()
    {
        GenerateEntityPrefab("Sine_Projectile", ColliderType.CIRCLE, _color: Color.red);
        GenerateEntityPrefab("Burst_Projectile", ColliderType.CIRCLE, _color: Color.blue);
        GenerateEntityPrefab("Even_Projectile", ColliderType.CIRCLE, _color: Color.green);
        GenerateEntityPrefab("Slow_Projectile", ColliderType.CIRCLE, _color: Color.yellow);

        foreach (EntityBehaviour _behaviorPrefab in editorEntityBehaviorScripObjs)
        {
            GenerateEntityBehaviorPrefab(_behaviorPrefab);
        }

        foreach (SpawnerPattern _pattern in editorSpawnerPatternScripObjs)
        {
            GenerateSpawnerPatternPrefab(_pattern);
        }

        foreach (SpawnerBehavior _behaviorPrefab in editorSpawnerBehaviorScripObjs)
        {
            GenerateSpawnerBehaviorPrefab(_behaviorPrefab);
        }
    }

    #region Entities
    public void GenerateEntityPrefab(string _entityType, ColliderType _colliderType, Sprite _sprite = null, Color? _color = null)
    {
        if (entityPrefabs.ContainsKey(_entityType))
        {
            Debug.LogError($"Failed to create new Entity Prefab as entity of type ({_entityType}) already exists!");
            return;
        }

        Entity newEntity = Instantiate(baseEntity);

        //Adds the correct default collider onto the new prefab

        //TODO: setup colliders
        switch (_colliderType)
        {
            case (ColliderType.CIRCLE):
                CircleCollider2D _circleCollider = newEntity.gameObject.AddComponent<CircleCollider2D>();

                _circleCollider.isTrigger = true;
                break;

            case (ColliderType.BOX):
                BoxCollider2D _boxCollider = newEntity.gameObject.AddComponent<BoxCollider2D>();

                _boxCollider.isTrigger = true;
                break;

            case (ColliderType.CAPSULE):
                CapsuleCollider2D _capsuleCollider = newEntity.gameObject.AddComponent<CapsuleCollider2D>();

                _capsuleCollider.isTrigger = true;
                break;

            case (ColliderType.EDGE):
                EdgeCollider2D _edgeCollider = newEntity.gameObject.AddComponent<EdgeCollider2D>();

                _edgeCollider.isTrigger = true;
                break;

            case (ColliderType.POLYGON):
                PolygonCollider2D _polygonCollider = newEntity.gameObject.AddComponent<PolygonCollider2D>();

                _polygonCollider.isTrigger = true;
                break;

            default:
                Debug.LogError($"Invalid ColliderType ({_colliderType}) while generating entity prefab for type ({_entityType}). Defaulting ColliderType to CIRCLE.");
                _colliderType = ColliderType.CIRCLE;

                CircleCollider2D _defaultCircleCollider = newEntity.gameObject.AddComponent<CircleCollider2D>();

                _defaultCircleCollider.isTrigger = true;
                break;
        }

        //Final Setup
        DontDestroyOnLoad(newEntity.gameObject);
        newEntity.name = _entityType + " Prefab";
        newEntity.SetupNewPrefab(_entityType, _colliderType, _sprite, _color);

        //Add the prefab to the dictionary
        entityPrefabs.Add(_entityType, newEntity);

        //Disables the prefab to avoid finding it during gameplay
        newEntity.gameObject.SetActive(false);

        Debug.Log($"Created entity prefab ({newEntity.name})");
    }

    /*
    public void GenerateEntityPrefab(string _entityType, ColliderType colliderType, SpriteRenderer _spriteRenderer = null)
    {
        if (entityPrefabs.ContainsKey(_entityType))
        {
            Debug.LogError($"Failed to create new Entity Prefab as entity of type ({_entityType}) already exists!");
            return;
        }
    }
    */

    //Called by EntityManager when it attempts to create a new entity
    //Returns a new copy of the desired entity
    public Entity InstantiateEntity(string _entityType)
    {
        if (entityPrefabs.TryGetValue(_entityType, out Entity _entityPrefab))
        {
            Entity newEntity = Instantiate(_entityPrefab);
            return newEntity;
        }
        else
        {
            //TODO: Maybe return some form of default entity?
            Debug.LogError($"Entity of type ({_entityType}) not found!");
            return null;
        }
    }
    #endregion

    #region Entity Behaviors
    //Adds a entity behavior to the list of available behaviors to copy. Generation of mod behaviors will occur elsewhere, perhaps TODO: in a [modname].cs file
    public void GenerateEntityBehaviorPrefab(EntityBehaviour _behavior)
    {
        if (entityBehaviors.ContainsKey(_behavior.EntityBehaviorName))
        {
            Debug.LogError($"Failed to create new Entity Behavior as behavior of type ({_behavior.EntityBehaviorName}) already exists!");
            return;
        }

        Debug.Log($"Created entity behavior ({_behavior.EntityBehaviorName})");
        entityBehaviors.Add(_behavior.EntityBehaviorName, _behavior);
    }

    //Returns a new copy of the desired entity behavior
    public EntityBehaviour InstantiateEntityBehavior(string _behaviorName)
    {
        if(entityBehaviors.TryGetValue(_behaviorName, out EntityBehaviour _entityBehavior))
        {
            EntityBehaviour _newBehavior = Instantiate(_entityBehavior);
            _newBehavior.Init();
            return _newBehavior;
        }

        Debug.LogError($"Entity Behavior of setup ({_behaviorName}) does not exist!");
        return null;
    }
    #endregion

    #region Spawner Behaviors
    //Adds a spawner behavior to the list of available behaviors to copy. Generation of mod behaviors will occur elsewhere, perhaps TODO: in a [modname].cs file
    public void GenerateSpawnerBehaviorPrefab(SpawnerBehavior _behavior)
    {
        if (spawnerBehaviors.ContainsKey(_behavior.SpawnerBehaviorName))
        {
            Debug.LogError($"Failed to create new Spawner Behavior as behavior of type ({_behavior.SpawnerBehaviorName}) already exists!");
            return;
        }

        Debug.Log($"Created spawner behavior ({_behavior.SpawnerBehaviorName})");
        spawnerBehaviors.Add(_behavior.SpawnerBehaviorName, _behavior);
    }

    //Returns a new copy of the desired spawner behavior
    public SpawnerBehavior InstantiateSpawnerBehavior(string _behaviorName)
    {
        if (spawnerBehaviors.TryGetValue(_behaviorName, out SpawnerBehavior _behavior))
        {
            SpawnerBehavior _newBehavior = Instantiate(_behavior);
            _newBehavior.Init();
            return _newBehavior;
        }

        Debug.LogError($"Spawner Behavior of setup ({_behaviorName}) does not exist!");
        return null;
    }
    #endregion

    #region SpawnerPatterns
    //Adds a entity pattern to the list of available behaviors to copy. Generation of mod patterns will occur elsewhere, perhaps TODO: in a [modname].cs file
    public void GenerateSpawnerPatternPrefab(SpawnerPattern _pattern)
    {
        if (spawnerPatterns.ContainsKey(_pattern.patternType))
        {
            Debug.LogError($"Failed to create new Entity Behavior as behavior of type ({_pattern.patternType}) already exists!");
            return;
        }

        Debug.Log($"Created entity pattern ({_pattern.patternType})");
        spawnerPatterns.Add(_pattern.patternType, _pattern);
    }

    //Returns a new copy of the desired pattern
    public SpawnerPattern GetSpawnerPattern(string _patternType)
    {
        if (spawnerPatterns.TryGetValue(_patternType, out SpawnerPattern _spawnerPattern))
        {
            return Instantiate(_spawnerPattern);
        }

        Debug.LogError($"Entity Pattern of type ({_patternType}) does not exist!");
        return null;
    }
    #endregion
}