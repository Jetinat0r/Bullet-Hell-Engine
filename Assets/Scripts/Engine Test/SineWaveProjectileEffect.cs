using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Sine Wave Projectile Effect", menuName = "ScriptableObjects/Projectile Effects/Sine Wave Projectile Effect")]
public class SineWaveProjectileEffect : ProjectileEffect
{
    [Space(10f)]
    [Header("Sine Wave Vars")]
    //How often waves appear
    public float frequency = 1f;
    //Max distance from the sin wave's "center"
    public float magnitude = 0.5f;
    //Where the projectile is on the curve
    private float sinPos = 0f;
    //How far from "normal" the wave is
    private float lastPosition;

    //Used to reset local values on copy
    public override void Init()
    {
        base.Init();
        sinPos = 0f;
        lastPosition = 0f;
    }

    //Assigns effects to the correct places in Projectile's sequence of events
    public override void AddEffects(Projectile projectile)
    {
        if (!hasAppliedEffects)
        {
            //Apply Effects
            //Debug.Log("Applying Effects!");

            projectile.onMoveCalculationEvents += ChangeMovement;

            hasAppliedEffects = true;
        }

        //Initialize sin position and lastOffset
        sinPos = 0f;
        lastPosition = 0f;
    }

    //Removes the effects assigned in AddEffects()
    public override void RemoveEffects(Projectile projectile)
    {
        if (isPermanent)
        {
            //Debug.Log("Permanent effect, not removing");
            return;
        }

        //Remove Effects
        //Debug.Log("RemovingEffects");

        projectile.onMoveCalculationEvents -= ChangeMovement;

        //Destroy component after removing effects since it is not permanent
        //Destroy(this);
    }

    private void ChangeMovement(Projectile projectile)
    {
        float newPosition = Mathf.Sin(sinPos * frequency) * Time.fixedDeltaTime;

        //Ends up moving the pos up or down by the difference between the old position and the new position to create the offset to follow the sine wave
        projectile.nextPos += projectile.transform.up * (newPosition - lastPosition) * magnitude;

        lastPosition = newPosition;
        sinPos += Time.fixedDeltaTime;
    }
}
