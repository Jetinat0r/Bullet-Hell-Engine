using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FpsCounter : MonoBehaviour
{
    public TMP_Text fpsText;

    private int lastFrameIndex;
    private float[] frameDeltaTimeArray;

    private void Awake()
    {
        lastFrameIndex = 0;
        frameDeltaTimeArray = new float[fpsText.text.Length];
    }

    // Update is called once per frame
    void Update()
    {
        frameDeltaTimeArray[lastFrameIndex] = Time.deltaTime;
        lastFrameIndex = (lastFrameIndex + 1) % fpsText.text.Length;

        fpsText.text = "FPS: " + Mathf.RoundToInt(CalculateFps()).ToString();
    }

    private float CalculateFps()
    {
        float total = 0f;

        foreach(float dT in frameDeltaTimeArray)
        {
            total += dT;
        }

        return frameDeltaTimeArray.Length / total;
    }
}
