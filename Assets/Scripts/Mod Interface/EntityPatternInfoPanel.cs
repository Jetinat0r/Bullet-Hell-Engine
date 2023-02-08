using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnerPatternInfoPanel : MonoBehaviour
{
    [SerializeField]
    public TMP_Text patternNameText;

    private string storedPatternName;
    private int storedIndex;

    public void Init(string patternName, int index = -1)
    {
        patternNameText.text = patternName;
        storedPatternName = patternName;

        storedIndex = index;
    }

    public void AddPattern()
    {
        ModTester.instance.AddSpawnerPattern(storedPatternName);
    }

    public void RemovePattern()
    {
        ModTester.instance.RemoveSpawnerPattern(storedIndex);
    }
}