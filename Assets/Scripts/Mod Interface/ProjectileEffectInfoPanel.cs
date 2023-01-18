using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProjectileEffectInfoPanel : MonoBehaviour
{
    [SerializeField]
    public TMP_Text effectName;
    [SerializeField]
    public TMP_Text effectType;
    private string storedEffectName;

    [SerializeField]
    private TMP_Text effectCountText;
    private int effectCount = 0;


    public void Init(ProjectileEffect e)
    {
        effectName.text = "Name: " + e.projectileEffectName;
        effectType.text = "Type: " + e.projectileEffectType;

        storedEffectName = e.projectileEffectName;
    }

    public void IncreaseCount()
    {
        ModTester.instance.AddProjectileEffect(storedEffectName);

        effectCount++;
        effectCountText.text = effectCount.ToString();
    }

    public void DecreaseCount()
    {
        if(effectCount == 0)
        {
            return;
        }

        ModTester.instance.RemoveProjectileEffect(storedEffectName);

        effectCount--;
        effectCountText.text = effectCount.ToString();
    }
}