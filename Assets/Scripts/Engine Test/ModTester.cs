using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModTester : MonoBehaviour
{
    [SerializeField]
    private ProjectileSpawner spawner;

    [SerializeField]
    private GameObject modTestPanel;

    [SerializeField]
    private TMP_InputField projectileNameField;
    [SerializeField]
    private TMP_InputField projectileEffectNameField;
    [SerializeField]
    private TMP_InputField spawnerEffectNameField;

    [SerializeField]
    private Toggle isSpawnerEnabledToggle;

    private bool isSpawnerEnabled = false;
    private bool isUIShown = true;

    private void Awake()
    {
        isSpawnerEnabledToggle.interactable = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            ToggleUI();
        }
    }

    public void UpdateProjectileName()
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);


        spawner.spawnerProjectileType = projectileNameField.text;
    }

    public void AddProjectileEffect()
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        spawner.defaultProjectileEffects.Add(projectileEffectNameField.text);
    }

    public void AddSpawnerEffect()
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        spawner.defaultSpawnerEffects.Add(spawnerEffectNameField.text);
    }

    public void ToggleSpawnerState()
    {
        if (isSpawnerEnabled)
        {
            spawner.Disable();
            isSpawnerEnabled = false;
        }
        else
        {
            //spawner.Enable(true);
            spawner.Init();
            isSpawnerEnabled = true;
        }

        isSpawnerEnabledToggle.SetIsOnWithoutNotify(isSpawnerEnabled);
    }

    public void SaveLog()
    {
        LogSaver.instance.SaveLogToFile();
    }

    public void CopyModPathToClipboard()
    {
        TextEditor te = new TextEditor();
        te.text = Application.persistentDataPath;
        te.SelectAll();
        te.Copy();
    }

    public void ToggleUI()
    {
        isUIShown = !isUIShown;
        modTestPanel.SetActive(isUIShown);
    }
}
