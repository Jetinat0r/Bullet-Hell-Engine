using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollPopulator : MonoBehaviour
{
    [SerializeField]
    public RectTransform Content;
    [SerializeField]
    private RectTransform ElementPrefab;
    [SerializeField]
    private int numElements;

    [SerializeField]
    private float spacing;
    //Derived at runtime
    private float heightOfElement;

    public List<GameObject> elements = new List<GameObject>();


    public void GenerateElements()
    {
        heightOfElement = ElementPrefab.rect.height + spacing;

        Content.sizeDelta = new Vector2(Content.sizeDelta.x, heightOfElement * numElements);
        Content.anchoredPosition = new Vector2(0, 0);

        for (int i = 0; i < numElements; i++)
        {
            RectTransform curElement = Instantiate(ElementPrefab, Content.transform);
            curElement.anchoredPosition = new Vector2(0, -i * heightOfElement - (heightOfElement / 2));
            curElement.name = $"Prefab {i}";

            elements.Add(curElement.gameObject);
        }
    }

    public void SetNumElements(int newNum)
    {
        if(newNum == numElements)
        {
            return;
        }

        foreach(GameObject element in elements)
        {
            Destroy(element);
        }
        elements.Clear();

        numElements = newNum;
        GenerateElements();
    }
}
