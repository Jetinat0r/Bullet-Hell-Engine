using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopulateScroll : MonoBehaviour
{
    [SerializeField]
    public RectTransform Content;
    [SerializeField]
    private RectTransform EffectPrefab;
    [SerializeField]
    private int numElements;

    [SerializeField]
    private float spacing;

    private float heightOfElement;

    // Start is called before the first frame update
    void Start()
    {
        heightOfElement = EffectPrefab.rect.height + spacing;

        Content.sizeDelta = new Vector2(Content.rect.width, heightOfElement * numElements);
        Content.anchoredPosition = new Vector2(0, 0);

        for(int i = 0; i < numElements; i++)
        {
            RectTransform curEffect = Instantiate(EffectPrefab, Content.transform);
            curEffect.anchoredPosition = new Vector2(0, -i * heightOfElement - (heightOfElement / 2));
            curEffect.name = $"Prefab {i}";
        }
    }
}
