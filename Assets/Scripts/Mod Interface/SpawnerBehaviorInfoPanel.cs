using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpawnerBehaviorInfoPanel : MonoBehaviour
{
    [SerializeField]
    public TMP_Text behaviorName;
    [SerializeField]
    public TMP_Text behaviorType;
    private string storedBehaviorName;

    [SerializeField]
    private TMP_Text behaviorCountText;
    private int behaviorCount = 0;


    public void Init(SpawnerBehavior e)
    {
        behaviorName.text = "Name: " + e.SpawnerBehaviorName;
        behaviorType.text = "Type: " + e.GetSpawnerBehaviorType();

        storedBehaviorName = e.SpawnerBehaviorName;
    }

    public void IncreaseCount()
    {
        ModTester.instance.AddSpawnerBehavior(storedBehaviorName);

        behaviorCount++;
        behaviorCountText.text = behaviorCount.ToString();
    }

    public void DecreaseCount()
    {
        if (behaviorCount == 0)
        {
            return;
        }

        ModTester.instance.RemoveSpawnerBehavior(storedBehaviorName);

        behaviorCount--;
        behaviorCountText.text = behaviorCount.ToString();
    }
}