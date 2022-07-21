using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//The base class from which all mods will derive from
//This is primarily meant for loading a mod's projectiles, effects, prefabs, etc.
//TODO: Write somewhere that "Modded projectiles and such will NOT derive from a class like ModProjectile, they will just derive from Projectile or the relevant class"
public class Mod
{
    //Called when the mod is first loaded in
    public virtual void OnLoad()
    {
        return;
    }

    //Called after every mod has been loaded in, should be used to coordinate with other mods
    public virtual void OnLateLoad()
    {
        return;
    }
}