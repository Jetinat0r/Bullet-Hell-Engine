using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager instance;

    //Dictionary of key=ProjectileEffect.projectileEffectName; value=ProjectileEffect ScriptableObject
    public Dictionary<string, List<ProjectileEffect>> activeProjectileEffects = new Dictionary<string, List<ProjectileEffect>>();
    public Dictionary<string, List<ProjectileEffect>> inactiveProjectileEffects = new Dictionary<string, List<ProjectileEffect>>();

    //Dictionary of key=ProjectileEffect.projectileEffectName; value=ProjectileEffect ScriptableObject
    public Dictionary<string, List<SpawnerEffect>> activeSpawnerEffects = new Dictionary<string, List<SpawnerEffect>>();
    public Dictionary<string, List<SpawnerEffect>> inactiveSpawnerEffects = new Dictionary<string, List<SpawnerEffect>>();

    //Creates a singleton of EffectManager
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

    #region Projectile Effects
    //Creates a new projectile ffect via CachedBHEResources based off of the given projectileEffectName and initializes it
    public ProjectileEffect CreateNewProjectileEffect(string _projectileEffectName)
    {
        Debug.Log($"Creating new effect ({_projectileEffectName})");
        //Ensures that there is a list in activeProjectiles to receive the given projectile
        if (!activeProjectileEffects.ContainsKey(_projectileEffectName))
        {
            activeProjectileEffects.Add(_projectileEffectName, new List<ProjectileEffect>());
        }

        //Adds the projectile to activeProjectiles and puts it in the spawn order
        ProjectileEffect newProjectileEffect = CachedBHEResources.instance.InstantiateProjectileEffect(_projectileEffectName);

        if (newProjectileEffect == null)
        {
            Debug.LogError($"Failed to add new projectile effect of name ({_projectileEffectName})");
            return null;
        }

        //Adds the projectile effect to the pool
        activeProjectileEffects[_projectileEffectName].Add(newProjectileEffect);

        newProjectileEffect.Init();
        return newProjectileEffect;
    }

    //Returns a projectile via the specified projectileEffectName and Initializes it
    //If there is a disable projectile effect, that one will take priority over instantiating a new one
    public ProjectileEffect GetProjectileEffect(string _projectileEffectName)
    {
        //Checks if there is a list in inactiveProjectileEffects for the given projectileEffectName
        if (inactiveProjectileEffects.TryGetValue(_projectileEffectName, out List<ProjectileEffect> projectileEffects))
        {
            //Ensures that there is a valid inactive projectile effect to grab
            if (projectileEffects.Count == 0)
            {
                return CreateNewProjectileEffect(_projectileEffectName);
            }

            //Grabs a projectile effect
            ProjectileEffect projectileEffect = projectileEffects[0];
            projectileEffects.RemoveAt(0);

            //Ensures that a list exists in the activeProjectileEffects dictionary to receive the new projectile effect
            //If it doesn't exist, creates one, and then either way continues to add the projectile effect to that list
            //This should never run, but is here just in case
            if (!activeProjectileEffects.ContainsKey(_projectileEffectName))
            {
                activeProjectileEffects.Add(_projectileEffectName, new List<ProjectileEffect>());
            }
            activeProjectileEffects[_projectileEffectName].Add(projectileEffect);

            projectileEffect.Init();
            return projectileEffect;
        }
        else
        {
            return CreateNewProjectileEffect(_projectileEffectName);
        }
    }

    //Moves a projectile effect from the activeProjectileEffects dictionary to the inactiveProjectileEffects dictionary
    public void DeactivateProjectileEffect(ProjectileEffect projectileEffect)
    {
        //Ensures that there is a list in inactiveProjectileEffects to receive the given projectile effect
        if (!inactiveProjectileEffects.ContainsKey(projectileEffect.projectileEffectName))
        {
            inactiveProjectileEffects.Add(projectileEffect.projectileEffectName, new List<ProjectileEffect>());
        }

        //Removes the projectile effect from activeProjectileEffects and adds it to it's corresponding list in inactiveProjectileEffects
        activeProjectileEffects[projectileEffect.projectileEffectName].Remove(projectileEffect);
        inactiveProjectileEffects[projectileEffect.projectileEffectName].Add(projectileEffect);
    }

    //Clears all projectile effect pools
    public void ClearProjectileEffectPools()
    {
        //Clears the dictionary of and destroys all active projectile effects
        foreach (string key in activeProjectileEffects.Keys)
        {
            foreach (ProjectileEffect pe in activeProjectileEffects[key])
            {
                Destroy(pe);
            }

            activeProjectileEffects[key].Clear();
        }
        activeProjectileEffects.Clear();

        //Clears the dictionary of and destroys all inactive projectile effects
        foreach (string key in inactiveProjectileEffects.Keys)
        {
            foreach (ProjectileEffect pe in inactiveProjectileEffects[key])
            {
                Destroy(pe);
            }

            inactiveProjectileEffects[key].Clear();
        }
        inactiveProjectileEffects.Clear();
    }
    #endregion

    #region Spawner Effects
    //Creates a new spawner effect via CachedBHEResources based off of the given spawnerEffectName and initializes it
    public SpawnerEffect CreateNewSpawnerEffect(string _spawnerEffectName)
    {
        Debug.Log($"Creating new effect ({_spawnerEffectName})");

        //Ensures that there is a list in activeSpawnerEffectss to receive the given spawner effect
        if (!activeSpawnerEffects.ContainsKey(_spawnerEffectName))
        {
            activeSpawnerEffects.Add(_spawnerEffectName, new List<SpawnerEffect>());
        }

        //Adds the spawner effect to activeSpawnerEffectss and puts it in the spawn order
        SpawnerEffect newSpawnerEffect = CachedBHEResources.instance.InstantiateSpawnerEffect(_spawnerEffectName);

        if (newSpawnerEffect == null)
        {
            Debug.LogError($"Failed to add new spawner effect of name ({_spawnerEffectName})");
            return null;
        }

        //Adds the spawner effect to the pool
        activeSpawnerEffects[_spawnerEffectName].Add(newSpawnerEffect);

        newSpawnerEffect.Init();
        return newSpawnerEffect;
    }

    //Returns a projectile via the specified spawnerEffectName and Initializes it
    //If there is a disabled spawner effect, that one will take priority over instantiating a new one
    public SpawnerEffect GetSpawnerEffect(string _spawnerEffectName)
    {
        //Checks if there is a list in inactiveSpawnerEffects for the given spawnerEffectName
        if (inactiveSpawnerEffects.TryGetValue(_spawnerEffectName, out List<SpawnerEffect> spawnerEffects))
        {
            //Ensures that there is a valid inactive spawner effect to grab
            if (spawnerEffects.Count == 0)
            {
                return CreateNewSpawnerEffect(_spawnerEffectName);
            }

            //Grabs a projectile effect
            SpawnerEffect spawnerEffect = spawnerEffects[0];
            spawnerEffects.RemoveAt(0);

            //Ensures that a list exists in the activeSpawnerEffects dictionary to receive the new spawner effect
            //If it doesn't exist, creates one, and then either way continues to add the spawner effect to that list
            //This should never run, but is here just in case
            if (!activeSpawnerEffects.ContainsKey(_spawnerEffectName))
            {
                activeSpawnerEffects.Add(_spawnerEffectName, new List<SpawnerEffect>());
            }
            activeSpawnerEffects[_spawnerEffectName].Add(spawnerEffect);

            spawnerEffect.Init();
            return spawnerEffect;
        }
        else
        {
            return CreateNewSpawnerEffect(_spawnerEffectName);
        }
    }

    //Moves a projectile effect from the activeSpawnerEffects dictionary to the inactiveSpawnerEffects dictionary
    public void DeactivateSpawnerEffect(SpawnerEffect spawnerEffect)
    {
        //Ensures that there is a list in inactiveSpawnerEffects to receive the given spawner effect
        if (!inactiveSpawnerEffects.ContainsKey(spawnerEffect.spawnerEffectName))
        {
            inactiveSpawnerEffects.Add(spawnerEffect.spawnerEffectName, new List<SpawnerEffect>());
        }

        //Removes the projectile from activeSpawnerEffects and adds it to it's corresponding list in inactiveSpawnerEffects
        activeSpawnerEffects[spawnerEffect.spawnerEffectName].Remove(spawnerEffect);
        inactiveSpawnerEffects[spawnerEffect.spawnerEffectName].Add(spawnerEffect);
    }

    //Clears all projectile effect pools
    public void ClearSpawnerEffectPools()
    {
        //Clears the dictionary of and destroys all active spawner effects
        foreach (string key in activeProjectileEffects.Keys)
        {
            foreach (SpawnerEffect pe in activeSpawnerEffects[key])
            {
                Destroy(pe);
            }

            activeSpawnerEffects[key].Clear();
        }
        activeSpawnerEffects.Clear();

        //Clears the dictionary of and destroys all inactive spawner effects
        foreach (string key in inactiveSpawnerEffects.Keys)
        {
            foreach (SpawnerEffect pe in inactiveSpawnerEffects[key])
            {
                Destroy(pe);
            }

            inactiveSpawnerEffects[key].Clear();
        }
        inactiveSpawnerEffects.Clear();
    }
    #endregion


    //Clears all effect pools
    public void ClearAllEffectPools()
    {
        ClearProjectileEffectPools();

        ClearSpawnerEffectPools();
    }
}
