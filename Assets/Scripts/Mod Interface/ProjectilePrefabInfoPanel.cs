using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProjectilePrefabInfoPanel : MonoBehaviour
{
    [SerializeField]
    public TMP_Text prefabNameText;

    private string storedPrefabName;

    public void Init(string prefabName)
    {
        prefabNameText.text = prefabName;
        storedPrefabName = prefabName;
    }

    public void SetProjectileType()
    {
        ModTester.instance.UpdateProjectileName(storedPrefabName);
    }
}
