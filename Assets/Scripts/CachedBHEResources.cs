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

    private void Awake()
    {
        if(instance == null)
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
        GenerateProjectilePrefab("Sine_Projectile", ColliderType.CIRCLE, _color:Color.red);
        GenerateProjectilePrefab("Burst_Projectile", ColliderType.CIRCLE, _color: Color.blue);
        GenerateProjectilePrefab("Even_Projectile", ColliderType.CIRCLE, _color: Color.green);
        GenerateProjectilePrefab("Slow_Projectile", ColliderType.CIRCLE, _color: Color.yellow);
    }

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
                CircleCollider2D  _circleCollider = newProjectile.gameObject.AddComponent<CircleCollider2D>();

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
    }

    public void GenerateProjectilePrefab(string _projectileType, ColliderType colliderType, SpriteRenderer _spriteRenderer = null)
    {
        if (projectilePrefabs.ContainsKey(_projectileType))
        {
            Debug.LogError($"Failed to create new Projectile Prefab as projectile of type ({_projectileType}) already exists!");
            return;
        }
    }

    //Called by ProjectileManager when it attempts to create a new projectile
    public Projectile InstantiateProjectile(string _projectileType)
    {
        if(projectilePrefabs.TryGetValue(_projectileType, out Projectile _projectilePrefab))
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
}