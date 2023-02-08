using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityManager : MonoBehaviour
{
    public static EntityManager instance;

    //Dictionary of key=Entity.entityType; value=Entity w/ attatched GameObject
    public Dictionary<string, List<Entity>> activeEntities = new Dictionary<string, List<Entity>>();
    public Dictionary<string, List<Entity>> inactiveEntities = new Dictionary<string, List<Entity>>();
    //The maximum number of entities allowed to be active at any given time
    public int maxEntities = 10000;
    //A list of the active entities in the order that they spawned, so that if too many entities spawn I can just force remove some
    private List<Entity> activeEntitySpawnOrder = new List<Entity>();

    //Creates a singleton of EntityManager
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

    //Creates a new entity via CachedBHEResources based off of the given entityType
    public Entity CreateNewEntity(string _entityType)
    {
        //Ensures that there is room for the entity to exist
        while(activeEntitySpawnOrder.Count >= maxEntities)
        {
            DeactivateEntity(activeEntitySpawnOrder[0]);
        }

        //Ensures that there is a list in activeEntities to receive the given entity
        if (!activeEntities.ContainsKey(_entityType))
        {
            activeEntities.Add(_entityType, new List<Entity>());
        }

        //Adds the entity to activeEntities and puts it in the spawn order
        Entity newEntity = CachedBHEResources.instance.InstantiateEntity(_entityType);

        if (newEntity == null)
        {
            Debug.LogError($"Failed to add new entity of type ({_entityType})");
            return null;
        }

        //Enables the entity as if was pooled or instantiated it would be disabled
        newEntity.gameObject.SetActive(true);

        //Adds the entity to the pool
        activeEntities[_entityType].Add(newEntity);
        activeEntitySpawnOrder.Add(newEntity);

        return newEntity;
    }

    //Returns a entity via the specified entityType and sets "isActive" to true
    //If there is a disable entity, that one will take priority over instantiating a new one
    public Entity GetEntity(string _entityType)
    {
        //Checks if there is a list in inactiveEntities for the given entityType
        if (inactiveEntities.TryGetValue(_entityType, out List<Entity> entities))
        {
            //Ensures that there is a valid inactive entity to grab
            if(entities.Count == 0)
            {
                return CreateNewEntity(_entityType);
            }

            //Grabs a entity
            Entity entity = entities[0];
            entities.RemoveAt(0);

            //Ensures that a list exists in the activeEntities dictionary to receive the new entity
            //If it doesn't exist, creates one, and then either way continues to add the entity to that list
            //This should never run, but is here just in case
            if (!activeEntities.ContainsKey(_entityType))
            {
                activeEntities.Add(_entityType, new List<Entity>());
            }
            activeEntities[_entityType].Add(entity);

            //Ensures that there is room for the entity to exist
            while (activeEntitySpawnOrder.Count >= maxEntities)
            {
                DeactivateEntity(activeEntitySpawnOrder[0]);
            }

            //Sets the entity to active, adds it to the spawn order, and returns it
            entity.gameObject.SetActive(true);
            activeEntitySpawnOrder.Add(entity);
            return entity;
        }
        else
        {
            return CreateNewEntity(_entityType);
        }
    }

    //Moves a entity from the activeEntities dictionary to the inactiveEntities dictionary and sets "isActive" to false
    public void DeactivateEntity(Entity entity)
    {
        //Ensures that there is a list in inactiveEntities to receive the given entity
        if (!inactiveEntities.ContainsKey(entity.entityType))
        {
            inactiveEntities.Add(entity.entityType, new List<Entity>());
        }

        //Removes the entity from activeEntities, deactivates the entity, removes it from the spawn order, and adds it to it's corresponding list in inactiveEntities
        activeEntities[entity.entityType].Remove(entity);
        inactiveEntities[entity.entityType].Add(entity);
        activeEntitySpawnOrder.Remove(entity);
        entity.gameObject.SetActive(false);
    }

    //Clears all entity pools
    public void ClearEntityPools()
    {
        //Clears the dictionary of and destroys all active entities
        foreach(string key in activeEntities.Keys)
        {
            foreach(Entity p in activeEntities[key])
            {
                Destroy(p.gameObject);
            }

            activeEntities[key].Clear();
        }
        activeEntities.Clear();

        //Clears the dictionary of and destroys all inactive entities
        foreach (string key in inactiveEntities.Keys)
        {
            foreach (Entity p in inactiveEntities[key])
            {
                Destroy(p.gameObject);
            }

            inactiveEntities[key].Clear();
        }
        inactiveEntities.Clear();
    }
}
