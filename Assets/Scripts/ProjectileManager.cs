using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileManager : MonoBehaviour
{
    public static ProjectileManager instance;

    //Dictionary of key=Projectile.projectileType; value=Projectile w/ attatched GameObject
    public Dictionary<string, List<Projectile>> activeProjectiles = new Dictionary<string, List<Projectile>>();
    public Dictionary<string, List<Projectile>> inactiveProjectiles = new Dictionary<string, List<Projectile>>();
    //The maximum number of projectiles allowed to be active at any given time
    public int maxProjectiles = 10000;
    //A list of the active projectiles in the order that they spawned, so that if too many projectiles spawn I can just force remove some
    private List<Projectile> activeProjectileSpawnOrder = new List<Projectile>();

    //Creates a singleton of ProjectileManager
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

    //Creates a new projectile via CachedBHEResources based off of the given projectileType
    public Projectile CreateNewProjectile(string _projectileType)
    {
        //Ensures that there is room for the projectile to exist
        while(activeProjectileSpawnOrder.Count >= maxProjectiles)
        {
            DeactivateProjectile(activeProjectileSpawnOrder[0]);
        }

        //Ensures that there is a list in activeProjectiles to receive the given projectile
        if (!activeProjectiles.ContainsKey(_projectileType))
        {
            activeProjectiles.Add(_projectileType, new List<Projectile>());
        }

        //Adds the projectile to activeProjectiles and puts it in the spawn order
        Projectile newProjectile = CachedBHEResources.instance.InstantiateProjectile(_projectileType);

        if (newProjectile == null)
        {
            Debug.LogError($"Failed to add new projectile of type ({_projectileType})");
            return null;
        }

        //Enables the projectile as if was pooled or instantiated it would be disabled
        newProjectile.gameObject.SetActive(true);

        //Adds the projectile to the pool
        activeProjectiles[_projectileType].Add(newProjectile);
        activeProjectileSpawnOrder.Add(newProjectile);

        return newProjectile;
    }

    //Returns a projectile via the specified projectileType and sets "isActive" to true
    //If there is a disable projectile, that one will take priority over instantiating a new one
    public Projectile GetProjectile(string _projectileType)
    {
        //Checks if there is a list in inactiveProjectiles for the given projectileType
        if (inactiveProjectiles.TryGetValue(_projectileType, out List<Projectile> projectiles))
        {
            //Ensures that there is a valid inactive projectile to grab
            if(projectiles.Count == 0)
            {
                return CreateNewProjectile(_projectileType);
            }

            //Grabs a projectile
            Projectile projectile = projectiles[0];
            projectiles.RemoveAt(0);

            //Ensures that a list exists in the activeProjectiles dictionary to receive the new projectile
            //If it doesn't exist, creates one, and then either way continues to add the projectile to that list
            //This should never run, but is here just in case
            if (!activeProjectiles.ContainsKey(_projectileType))
            {
                activeProjectiles.Add(_projectileType, new List<Projectile>());
            }
            activeProjectiles[_projectileType].Add(projectile);

            //Ensures that there is room for the projectile to exist
            while (activeProjectileSpawnOrder.Count >= maxProjectiles)
            {
                DeactivateProjectile(activeProjectileSpawnOrder[0]);
            }

            //Sets the projectile to active, adds it to the spawn order, and returns it
            projectile.gameObject.SetActive(true);
            activeProjectileSpawnOrder.Add(projectile);
            return projectile;
        }
        else
        {
            return CreateNewProjectile(_projectileType);
        }
    }

    //Moves a projectile from the activeProjectiles dictionary to the inactiveProjectiles dictionary and sets "isActive" to false
    public void DeactivateProjectile(Projectile projectile)
    {
        //Ensures that there is a list in inactiveProjectiles to receive the given projectile
        if (!inactiveProjectiles.ContainsKey(projectile.projectileType))
        {
            inactiveProjectiles.Add(projectile.projectileType, new List<Projectile>());
        }

        //Removes the projectile from activeProjectiles, deactivates the projectile, removes it from the spawn order, and adds it to it's corresponding list in inactiveProjectiles
        activeProjectiles[projectile.projectileType].Remove(projectile);
        inactiveProjectiles[projectile.projectileType].Add(projectile);
        activeProjectileSpawnOrder.Remove(projectile);
        projectile.gameObject.SetActive(false);
    }

    //Clears all projectile pools
    public void ClearProjectilePools()
    {
        //Clears the dictionary of and destroys all active projectiles
        foreach(string key in activeProjectiles.Keys)
        {
            foreach(Projectile p in activeProjectiles[key])
            {
                Destroy(p.gameObject);
            }

            activeProjectiles[key].Clear();
        }
        activeProjectiles.Clear();

        //Clears the dictionary of and destroys all inactive projectiles
        foreach (string key in inactiveProjectiles.Keys)
        {
            foreach (Projectile p in inactiveProjectiles[key])
            {
                Destroy(p.gameObject);
            }

            inactiveProjectiles[key].Clear();
        }
        inactiveProjectiles.Clear();
    }
}
