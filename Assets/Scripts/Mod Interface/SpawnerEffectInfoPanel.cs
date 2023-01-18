using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnerEffectInfoPanel : MonoBehaviour
{
    [SerializeField]
    public TMP_Text effectName;
    [SerializeField]
    public TMP_Text effectType;
    private string storedEffectName;

    [SerializeField]
    private TMP_Text effectCountText;
    private int effectCount = 0;


    public void Init(SpawnerEffect e)
    {
        effectName.text = "Name: " + e.spawnerEffectName;
        effectType.text = "Type: " + e.spawnerEffectType;

        storedEffectName = e.spawnerEffectName;
    }

    public void IncreaseCount()
    {
        ModTester.instance.AddSpawnerEffect(storedEffectName);

        effectCount++;
        effectCountText.text = effectCount.ToString();
    }

    public void DecreaseCount()
    {
        if (effectCount == 0)
        {
            return;
        }

        ModTester.instance.RemoveSpawnerEffect(storedEffectName);

        effectCount--;
        effectCountText.text = effectCount.ToString();
    }
}