using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileEffect : MonoBehaviour
{
    public bool isPermanent = false;
    protected bool hasAppliedEffects = false;

    //Assigns effects to the correct places in Projectile's sequence of events
    public virtual void AddEffects(Projectile projectile)
    {
        if (!hasAppliedEffects)
        {
            //Apply Effects
            Debug.Log("Applying Effects!");
            hasAppliedEffects = true;
        }
    }

    //Removes the effects assigned in AddEffects()
    public virtual void RemoveEffects(Projectile projectile)
    {
        if (isPermanent)
        {
            Debug.Log("Permanent effect, not removing");
            return;
        }

        //Remove Effects
        Debug.Log("RemovingEffects");

        //Destroy component after removing effects since it is not permanent
        Destroy(this);
    }
}
