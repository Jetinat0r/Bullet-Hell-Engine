using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EntityBehaviorInfoPanel : MonoBehaviour
{
    [SerializeField]
    public TMP_Text behaviorName;
    [SerializeField]
    public TMP_Text behaviorType;
    private string storedBehaviorName;

    [SerializeField]
    private TMP_Text behaviorCountText;
    private int behaviorCount = 0;


    public void Init(EntityBehaviour e)
    {
        behaviorName.text = "Name: " + e.EntityBehaviorName;
        behaviorType.text = "Type: " + e.GetEntityBehaviorType();

        storedBehaviorName = e.EntityBehaviorName;
    }

    public void IncreaseCount()
    {
        ModTester.instance.AddEntityBehavior(storedBehaviorName);

        behaviorCount++;
        behaviorCountText.text = behaviorCount.ToString();
    }

    public void DecreaseCount()
    {
        if(behaviorCount == 0)
        {
            return;
        }

        ModTester.instance.RemoveEntityBehavior(storedBehaviorName);

        behaviorCount--;
        behaviorCountText.text = behaviorCount.ToString();
    }
}