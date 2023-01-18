using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProjectilePatternInfoPanel : MonoBehaviour
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
        ModTester.instance.AddProjectilePattern(storedPatternName);
    }

    public void RemovePattern()
    {
        ModTester.instance.RemoveProjectilePattern(storedIndex);
    }
}