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

    //The base projectile from which every other projectile will generate its prefab from
    public Projectile baseProjectile;
    //Stores all base projectile prefabs, the key is the projectileType
    public Dictionary<string, Projectile> projectilePrefabs = new Dictionary<string, Projectile>();

    //List for me to add spawner effects to the dictionary
    [SerializeField]
    [Tooltip("These are for setting up spawner effects via the editor")]
    private List<SpawnerEffect> editorSpawnerEffectScripObjs = new List<SpawnerEffect>();
    //Every available Projectile Effect that spawners can choose from. The effects can still be modified after being copied over, but these are the defaults
    public Dictionary<string, SpawnerEffect> spawnerEffects = new Dictionary<string, SpawnerEffect>();

    //List for me to add projectile effects to the dictionary
    [SerializeField] [Tooltip("These are for setting up projectile effects via the editor")]
    private List<ProjectileEffect> editorProjectileEffectScripObjs = new List<ProjectileEffect>();
    //Every available Projectile Effect that spawners can choose from. The effects can still be modified after being copied over, but these are the defaults
    public Dictionary<string, ProjectileEffect> projectileEffects = new Dictionary<string, ProjectileEffect>();

    //List for me to add projectile patterns to the dictionary
    [SerializeField] [Tooltip("These are for setting up projectile effects via the editor")]
    private List<ProjectilePattern> editorProjectilePatternScripObjs = new List<ProjectilePattern>();
    //Every available Projectile Effect that spawners can choose from. The effects can still be modified after being copied over, but these are the defaults
    public Dictionary<string, ProjectilePattern> projectilePatterns = new Dictionary<string, ProjectilePattern>();

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
        GenerateProjectilePrefab("Sine_Projectile", ColliderType.CIRCLE, _color: Color.red);
        GenerateProjectilePrefab("Burst_Projectile", ColliderType.CIRCLE, _color: Color.blue);
        GenerateProjectilePrefab("Even_Projectile", ColliderType.CIRCLE, _color: Color.green);
        GenerateProjectilePrefab("Slow_Projectile", ColliderType.CIRCLE, _color: Color.yellow);

        foreach(ProjectileEffect _effectPrefab in editorProjectileEffectScripObjs)
        {
            GenerateProjectileEffectPrefab(_effectPrefab);
        }

        foreach(ProjectilePattern _pattern in editorProjectilePatternScripObjs)
        {
            GenerateProjectilePatternPrefab(_pattern);
        }

        foreach (SpawnerEffect _effectPrefab in editorSpawnerEffectScripObjs)
        {
            GenerateSpawnerEffectPrefab(_effectPrefab);
        }
    }

    #region Projectiles
    public void GenerateProjectilePrefab(string _projectileType, ColliderType _colliderType, Sprite _sprite = null, Color? _color = null)
    {
        if (projectilePrefabs.ContainsKey(_projectileType))
        {
            Debug.LogError($"Failed to create new Projectile Prefab as projectile of type ({_projectileType}) already exists!");
            return;
        }

        Projectile newProjectile = Instantiate(baseProjectile);

        //Adds the correct default collider onto the new prefab

        //TODO: setup colliders
        switch (_colliderType)
        {
            case (ColliderType.CIRCLE):
                CircleCollider2D _circleCollider = newProjectile.gameObject.AddComponent<CircleCollider2D>();

                _circleCollider.isTrigger = true;
                break;

            case (ColliderType.BOX):
                BoxCollider2D _boxCollider = newProjectile.gameObject.AddComponent<BoxCollider2D>();

                _boxCollider.isTrigger = true;
                break;

            case (ColliderType.CAPSULE):
                CapsuleCollider2D _capsuleCollider = newProjectile.gameObject.AddComponent<CapsuleCollider2D>();

                _capsuleCollider.isTrigger = true;
                break;

            case (ColliderType.EDGE):
                EdgeCollider2D _edgeCollider = newProjectile.gameObject.AddComponent<EdgeCollider2D>();

                _edgeCollider.isTrigger = true;
                break;

            case (ColliderType.POLYGON):
                PolygonCollider2D _polygonCollider = newProjectile.gameObject.AddComponent<PolygonCollider2D>();

                _polygonCollider.isTrigger = true;
                break;

            default:
                Debug.LogError($"Invalid ColliderType ({_colliderType}) while generating projectile prefab for type ({_projectileType}). Defaulting ColliderType to CIRCLE.");
                _colliderType = ColliderType.CIRCLE;

                CircleCollider2D _defaultCircleCollider = newProjectile.gameObject.AddComponent<CircleCollider2D>();

                _defaultCircleCollider.isTrigger = true;
                break;
        }

        //Final Setup
        DontDestroyOnLoad(newProjectile.gameObject);
        newProjectile.name = _projectileType + " Prefab";
        newProjectile.SetupNewPrefab(_projectileType, _colliderType, _sprite, _color);

        //Add the prefab to the dictionary
        projectilePrefabs.Add(_projectileType, newProjectile);

        //Disables the prefab to avoid finding it during gameplay
        newProjectile.gameObject.SetActive(false);

        Debug.Log($"Created projectile prefab ({newProjectile.name})");
    }

    /*
    public void GenerateProjectilePrefab(string _projectileType, ColliderType colliderType, SpriteRenderer _spriteRenderer = null)
    {
        if (projectilePrefabs.ContainsKey(_projectileType))
        {
            Debug.LogError($"Failed to create new Projectile Prefab as projectile of type ({_projectileType}) already exists!");
            return;
        }
    }
    */

    //Called by ProjectileManager when it attempts to create a new projectile
    //Returns a new copy of the desired projectile
    public Projectile InstantiateProjectile(string _projectileType)
    {
        if (projectilePrefabs.TryGetValue(_projectileType, out Projectile _projectilePrefab))
        {
            Projectile newProjectile = Instantiate(_projectilePrefab);
            return newProjectile;
        }
        else
        {
            //TODO: Maybe return some form of default projectile?
            Debug.LogError($"Projectile of type ({_projectileType}) not found!");
            return null;
        }
    }
    #endregion

    #region Projectile Effects
    //Adds a projectile effect to the list of available effects to copy. Generation of mod effects will occur elsewhere, perhaps TODO: in a [modname].cs file
    public void GenerateProjectileEffectPrefab(ProjectileEffect _effect)
    {
        if (projectileEffects.ContainsKey(_effect.projectileEffectType))
        {
            Debug.LogError($"Failed to create new Projectile Effect as effect of type ({_effect.projectileEffectType}) already exists!");
            return;
        }

        Debug.Log($"Created projectile effect ({_effect.projectileEffectType})");
        projectileEffects.Add(_effect.projectileEffectType, _effect);
    }

    //Returns a new copy of the desired projectile effect
    public ProjectileEffect GetProjectileEffect(string _effectType)
    {
        if(projectileEffects.TryGetValue(_effectType, out ProjectileEffect _projectileEffect))
        {
            return Instantiate(_projectileEffect);
        }

        Debug.LogError($"Projectile Effect of type ({_effectType}) does not exist!");
        return null;
    }
    #endregion

    #region Spawner Effects
    //Adds a spawner effect to the list of available effects to copy. Generation of mod effects will occur elsewhere, perhaps TODO: in a [modname].cs file
    public void GenerateSpawnerEffectPrefab(SpawnerEffect _effect)
    {
        if (spawnerEffects.ContainsKey(_effect.spawnerEffectType))
        {
            Debug.LogError($"Failed to create new Projectile Effect as effect of type ({_effect.spawnerEffectType}) already exists!");
            return;
        }

        Debug.Log($"Created spawner effect ({_effect.spawnerEffectType})");
        spawnerEffects.Add(_effect.spawnerEffectType, _effect);
    }

    //Returns a new copy of the desired spawner effect
    public SpawnerEffect GetSpawnerEffect(string _effectType)
    {
        if (spawnerEffects.TryGetValue(_effectType, out SpawnerEffect _effect))
        {
            SpawnerEffect _newEffect = Instantiate(_effect);
            _newEffect.Init();
            return _newEffect;
        }

        Debug.LogError($"Projectile Effect of type ({_effectType}) does not exist!");
        return null;
    }
    #endregion

    #region ProjectilePatterns
    //Adds a projectile pattern to the list of available effects to copy. Generation of mod patterns will occur elsewhere, perhaps TODO: in a [modname].cs file
    public void GenerateProjectilePatternPrefab(ProjectilePattern _pattern)
    {
        if (projectilePatterns.ContainsKey(_pattern.patternType))
        {
            Debug.LogError($"Failed to create new Projectile Effect as effect of type ({_pattern.patternType}) already exists!");
            return;
        }

        Debug.Log($"Created projectile pattern ({_pattern.patternType})");
        projectilePatterns.Add(_pattern.patternType, _pattern);
    }

    //Returns a new copy of the desired pattern
    public ProjectilePattern GetProjectilePattern(string _patternType)
    {
        if (projectilePatterns.TryGetValue(_patternType, out ProjectilePattern _projectilePattern))
        {
            return Instantiate(_projectilePattern);
        }

        Debug.LogError($"Projectile Pattern of type ({_patternType}) does not exist!");
        return null;
    }
    #endregion
}