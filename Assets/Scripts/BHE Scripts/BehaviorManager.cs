using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorManager : MonoBehaviour
{
    public static BehaviorManager instance;

    //Dictionary of key=EntityBehavior.entityBehaviorName; value=EntityBehavior ScriptableObject
    public Dictionary<string, List<EntityBehaviour>> activeEntityBehaviors = new Dictionary<string, List<EntityBehaviour>>();
    public Dictionary<string, List<EntityBehaviour>> inactiveEntityBehaviors = new Dictionary<string, List<EntityBehaviour>>();

    //Dictionary of key=EntityBehavior.entityBehaviorName; value=EntityBehavior ScriptableObject
    public Dictionary<string, List<SpawnerBehavior>> activeSpawnerBehaviors = new Dictionary<string, List<SpawnerBehavior>>();
    public Dictionary<string, List<SpawnerBehavior>> inactiveSpawnerBehaviors = new Dictionary<string, List<SpawnerBehavior>>();

    //Creates a singleton of BehaviorManager
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

    #region Behaviors
    //Creates a new entity ffect via CachedBHEResources based off of the given entityBehaviorName and initializes it
    public EntityBehaviour CreateNewEntityBehavior(string _entityBehaviorName)
    {
        Debug.Log($"Creating new behavior ({_entityBehaviorName})");
        //Ensures that there is a list in active
        //to receive the given entity
        if (!activeEntityBehaviors.ContainsKey(_entityBehaviorName))
        {
            activeEntityBehaviors.Add(_entityBehaviorName, new List<EntityBehaviour>());
        }

        //Adds the entity to activeEntities and puts it in the spawn order
        EntityBehaviour newEntityBehavior = CachedBHEResources.instance.InstantiateEntityBehavior(_entityBehaviorName);

        if (newEntityBehavior == null)
        {
            Debug.LogError($"Failed to add new entity behavior of name ({_entityBehaviorName})");
            return null;
        }

        //Adds the entity behavior to the pool
        activeEntityBehaviors[_entityBehaviorName].Add(newEntityBehavior);

        newEntityBehavior.Init();
        return newEntityBehavior;
    }

    //Returns a entity via the specified entityBehaviorName and Initializes it
    //If there is a disable entity behavior, that one will take priority over instantiating a new one
    public EntityBehaviour GetEntityBehavior(string _entityBehaviorName)
    {
        //Checks if there is a list in inactiveEntityBehaviors for the given entityBehaviorName
        if (inactiveEntityBehaviors.TryGetValue(_entityBehaviorName, out List<EntityBehaviour> entityBehaviors))
        {
            //Ensures that there is a valid inactive entity behavior to grab
            if (entityBehaviors.Count == 0)
            {
                return CreateNewEntityBehavior(_entityBehaviorName);
            }

            //Grabs a entity behavior
            EntityBehaviour entityBehavior = entityBehaviors[0];
            entityBehaviors.RemoveAt(0);

            //Ensures that a list exists in the activeEntityBehaviors dictionary to receive the new entity behavior
            //If it doesn't exist, creates one, and then either way continues to add the entity behavior to that list
            //This should never run, but is here just in case
            if (!activeEntityBehaviors.ContainsKey(_entityBehaviorName))
            {
                activeEntityBehaviors.Add(_entityBehaviorName, new List<EntityBehaviour>());
            }
            activeEntityBehaviors[_entityBehaviorName].Add(entityBehavior);

            entityBehavior.Init();
            return entityBehavior;
        }
        else
        {
            return CreateNewEntityBehavior(_entityBehaviorName);
        }
    }

    //Moves a entity behavior from the activeEntityBehaviors dictionary to the inactiveEntityBehaviors dictionary
    public void DeactivateEntityBehavior(EntityBehaviour entityBehavior)
    {
        //Ensures that there is a list in inactiveEntityBehaviors to receive the given entity behavior
        if (!inactiveEntityBehaviors.ContainsKey(entityBehavior.EntityBehaviorName))
        {
            inactiveEntityBehaviors.Add(entityBehavior.EntityBehaviorName, new List<EntityBehaviour>());
        }

        //Removes the entity behavior from activeEntityBehaviors and adds it to it's corresponding list in inactiveEntityBehaviors
        activeEntityBehaviors[entityBehavior.EntityBehaviorName].Remove(entityBehavior);
        inactiveEntityBehaviors[entityBehavior.EntityBehaviorName].Add(entityBehavior);
    }

    //Clears all entity behavior pools
    public void ClearEntityBehaviorPools()
    {
        //Clears the dictionary of and destroys all active entity behaviors
        foreach (string key in activeEntityBehaviors.Keys)
        {
            foreach (EntityBehaviour pe in activeEntityBehaviors[key])
            {
                Destroy(pe);
            }

            activeEntityBehaviors[key].Clear();
        }
        activeEntityBehaviors.Clear();

        //Clears the dictionary of and destroys all inactive entity behaviors
        foreach (string key in inactiveEntityBehaviors.Keys)
        {
            foreach (EntityBehaviour pe in inactiveEntityBehaviors[key])
            {
                Destroy(pe);
            }

            inactiveEntityBehaviors[key].Clear();
        }
        inactiveEntityBehaviors.Clear();
    }
    #endregion

    #region Spawner Behaviors
    //Creates a new spawner behavior via CachedBHEResources based off of the given spawnerBehaviorName and initializes it
    public SpawnerBehavior CreateNewSpawnerBehavior(string _spawnerBehaviorName)
    {
        Debug.Log($"Creating new behavior ({_spawnerBehaviorName})");

        //Ensures that there is a list in activeSpawnerBehaviorss to receive the given spawner behavior
        if (!activeSpawnerBehaviors.ContainsKey(_spawnerBehaviorName))
        {
            activeSpawnerBehaviors.Add(_spawnerBehaviorName, new List<SpawnerBehavior>());
        }

        //Adds the spawner behavior to activeSpawnerBehaviorss and puts it in the spawn order
        SpawnerBehavior newSpawnerBehavior = CachedBHEResources.instance.InstantiateSpawnerBehavior(_spawnerBehaviorName);

        if (newSpawnerBehavior == null)
        {
            Debug.LogError($"Failed to add new spawner behavior of name ({_spawnerBehaviorName})");
            return null;
        }

        //Adds the spawner behavior to the pool
        activeSpawnerBehaviors[_spawnerBehaviorName].Add(newSpawnerBehavior);

        newSpawnerBehavior.Init();
        return newSpawnerBehavior;
    }

    //Returns a entity via the specified spawnerBehaviorName and Initializes it
    //If there is a disabled spawner behavior, that one will take priority over instantiating a new one
    public SpawnerBehavior GetSpawnerBehavior(string _spawnerBehaviorName)
    {
        //Checks if there is a list in inactiveSpawnerBehaviors for the given spawnerBehaviorName
        if (inactiveSpawnerBehaviors.TryGetValue(_spawnerBehaviorName, out List<SpawnerBehavior> spawnerBehaviors))
        {
            //Ensures that there is a valid inactive spawner behavior to grab
            if (spawnerBehaviors.Count == 0)
            {
                return CreateNewSpawnerBehavior(_spawnerBehaviorName);
            }

            //Grabs a entity behavior
            SpawnerBehavior spawnerBehavior = spawnerBehaviors[0];
            spawnerBehaviors.RemoveAt(0);

            //Ensures that a list exists in the activeSpawnerBehaviors dictionary to receive the new spawner behavior
            //If it doesn't exist, creates one, and then either way continues to add the spawner behavior to that list
            //This should never run, but is here just in case
            if (!activeSpawnerBehaviors.ContainsKey(_spawnerBehaviorName))
            {
                activeSpawnerBehaviors.Add(_spawnerBehaviorName, new List<SpawnerBehavior>());
            }
            activeSpawnerBehaviors[_spawnerBehaviorName].Add(spawnerBehavior);

            spawnerBehavior.Init();
            return spawnerBehavior;
        }
        else
        {
            return CreateNewSpawnerBehavior(_spawnerBehaviorName);
        }
    }

    //Moves a entity behavior from the activeSpawnerBehaviors dictionary to the inactiveSpawnerBehaviors dictionary
    public void DeactivateSpawnerBehavior(SpawnerBehavior spawnerBehavior)
    {
        //Ensures that there is a list in inactiveSpawnerBehaviors to receive the given spawner behavior
        if (!inactiveSpawnerBehaviors.ContainsKey(spawnerBehavior.SpawnerBehaviorName))
        {
            inactiveSpawnerBehaviors.Add(spawnerBehavior.SpawnerBehaviorName, new List<SpawnerBehavior>());
        }

        //Removes the entity from activeSpawnerBehaviors and adds it to it's corresponding list in inactiveSpawnerBehaviors
        activeSpawnerBehaviors[spawnerBehavior.SpawnerBehaviorName].Remove(spawnerBehavior);
        inactiveSpawnerBehaviors[spawnerBehavior.SpawnerBehaviorName].Add(spawnerBehavior);
    }

    //Clears all entity behavior pools
    public void ClearSpawnerBehaviorPools()
    {
        //Clears the dictionary of and destroys all active spawner behaviors
        foreach (string key in activeEntityBehaviors.Keys)
        {
            foreach (SpawnerBehavior pe in activeSpawnerBehaviors[key])
            {
                Destroy(pe);
            }

            activeSpawnerBehaviors[key].Clear();
        }
        activeSpawnerBehaviors.Clear();

        //Clears the dictionary of and destroys all inactive spawner behaviors
        foreach (string key in inactiveSpawnerBehaviors.Keys)
        {
            foreach (SpawnerBehavior pe in inactiveSpawnerBehaviors[key])
            {
                Destroy(pe);
            }

            inactiveSpawnerBehaviors[key].Clear();
        }
        inactiveSpawnerBehaviors.Clear();
    }
    #endregion


    //Clears all behavior pools
    public void ClearAllBehaviorPools()
    {
        ClearEntityBehaviorPools();

        ClearSpawnerBehaviorPools();
    }
}
