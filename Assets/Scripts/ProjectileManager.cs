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

    //Adds the specified projectile to the pool of available bullets
    public void AddProjectile(Projectile projectile)
    {
        //Ensures that there is room for the projectile to exist
        while(activeProjectileSpawnOrder.Count >= maxProjectiles)
        {
            DeactivateProjectile(activeProjectileSpawnOrder[0]);
        }

        //Ensures that there is a list in inactiveProjectiles to receive the given projectile
        if (!activeProjectiles.ContainsKey(projectile.projectileType))
        {
            activeProjectiles.Add(projectile.projectileType, new List<Projectile>());
        }

        //Adds the projectile to activeProjectiles and puts it in the spawn order
        activeProjectiles[projectile.projectileType].Add(projectile);
        activeProjectileSpawnOrder.Add(projectile);
    }

    //Returns a disabled projectile from the specified pool and sets "isActive" to true
    //If no projectile is found, returns null
    public Projectile TryGetOldProjectile(string projectileType)
    {
        //Checks if there is a list in inactiveProjectiles for the given projectileType
        if (inactiveProjectiles.TryGetValue(projectileType, out List<Projectile> projectiles))
        {
            //Ensures that there is a valid inactive projectile to grab
            if(projectiles.Count == 0)
            {
                Debug.Log("No projectile found, returning null. [REMOVE MSG LATER]");
                return null;
            }

            //Grabs a projectile
            Projectile projectile = projectiles[0];
            projectiles.RemoveAt(0);

            //Ensures that a list exists in the activeProjectiles dictionary to receive the new projectile
            //If it doesn't exist, creates one, and then either way continues to add the projectile to that list
            //This should never run, but is here just in case
            if (!activeProjectiles.ContainsKey(projectileType))
            {
                activeProjectiles.Add(projectileType, new List<Projectile>());
            }
            activeProjectiles[projectileType].Add(projectile);

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

        Debug.Log("No projectile found, returning null. [REMOVE MSG LATER]");
        return null;
    }

    //Moves a projectile from the activeProjectiles dictionary to the inactiveProjectiles dictionary and sets "isActive" to false
    //TODO: Maybe call "ClearEffects?" maybe have that handle permanent effects and non-permanent ones?
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
    public void ClearProjectile()
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
