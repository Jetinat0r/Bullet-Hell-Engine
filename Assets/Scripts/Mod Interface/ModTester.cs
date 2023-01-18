using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class ModTester : MonoBehaviour
{
    public static ModTester instance;

    [SerializeField]
    private ProjectileSpawner spawner;

    public enum ModTesterPanels
    {
        INTRO = 0,
        CONTROLS = 1,
        PROJTYPE = 2,
        PROJEFFECTS = 3,
        SPAWNEREFFECTS = 4,
        PROJECTILEPATTERNS = 5,
    }

    //Base Panel
    [SerializeField]
    private GameObject modTestPanel;
    private bool isUIShown = true;

    [SerializeField]
    private int minFOV = 1;
    [SerializeField]
    private int maxFOV = 180;

    [Header("Intro Panel")]
    //Panel shown on startup
    [SerializeField]
    private GameObject introPanel;
    //Displays the current active state of the spawner
    [SerializeField]
    private Toggle isSpawnerEnabledToggle;
    private bool isSpawnerEnabled = false;

    [Header("Controls Panel")]
    //Panel containing controls to toggle the spawner on/off, save log, get mod path, and more <3
    [SerializeField]
    private GameObject controlsPanel;

    [Header("Projectile Prefab Panel")]
    //Panel controlling the current type of projectile being used
    [SerializeField]
    private GameObject projTypePanel;
    [SerializeField]
    private TMP_Text curProjTypeText;
    [SerializeField]
    private ScrollPopulator projTypeScrollSection;

    [Header("Projectile Effects Panel")]
    //Panel controlling what effects are applied to the projectiles spawned
    [SerializeField]
    private GameObject projEffectsPanel;
    [SerializeField]
    private ScrollPopulator projEffectsScrollSection;

    [Header("Spawner Effects Panel")]
    //Panel controlling what effects are applied to the spawner
    [SerializeField]
    private GameObject spawnerEffectsPanel;
    [SerializeField]
    private ScrollPopulator spawnerEffectsScrollSection;

    [Header("Projectile Patterns Panel")]
    //Panel controlling what patterns are used and in what order
    [SerializeField]
    private GameObject projectilePatternsPanel;
    [SerializeField]
    private ScrollPopulator activeProjectilePatternsScrollSection;
    [SerializeField]
    private ScrollPopulator availableProjectilePatternsScrollSection;
    [SerializeField]
    private Toggle randomizePatternsToggle;
    [SerializeField]
    private Toggle noRandomRepeatsToggle;

    //Local storage of pattern randomization bools
    //NOTE: if randomizePatterns is false, set noRandomRepeats to false
    //      if there is only one (1) pattern in use, set both to false
    private bool randomizePatterns = false;
    private bool noRandomRepeats = false;
    private List<string> activePatterns = new(new string[] { "Mod_Tester_Pattern" }); //Syntax Ouch!


    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        isSpawnerEnabledToggle.interactable = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        #region Quick Panel Access
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwapPanel(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwapPanel(2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SwapPanel(3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SwapPanel(4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SwapPanel(5);
        }
        #endregion

        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            ToggleUI();
        }

        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            ToggleSpawnerState();
        }

        if(Input.mouseScrollDelta.y > 0)
        {
            ZoomIn();
        }

        if(Input.mouseScrollDelta.y < 0)
        {
            ZoomOut();
        }
    }

    public void Init()
    {
        //Generate Projectile Type Buttons
        projTypeScrollSection.SetNumElements(CachedBHEResources.instance.projectilePrefabs.Count);
        string[] projectilePrefabNames = CachedBHEResources.instance.projectilePrefabs.Keys.ToArray();
        for(int i = 0; i < projectilePrefabNames.Length; i++)
        {
            //Wish I didn't have to use GetComponent
            projTypeScrollSection.elements[i].GetComponent<ProjectilePrefabInfoPanel>().Init(projectilePrefabNames[i]);
        }

        //Generate Projectile Effect Buttons
        projEffectsScrollSection.SetNumElements(CachedBHEResources.instance.projectileEffects.Count);
        ProjectileEffect[] projectileEffects = CachedBHEResources.instance.projectileEffects.Values.ToArray();
        for (int i = 0; i < projectileEffects.Length; i++)
        {
            //Wish I didn't have to use GetComponent
            projEffectsScrollSection.elements[i].GetComponent<ProjectileEffectInfoPanel>().Init(projectileEffects[i]);
        }

        //Generate Spawner Effect Buttons
        spawnerEffectsScrollSection.SetNumElements(CachedBHEResources.instance.spawnerEffects.Count);
        SpawnerEffect[] spawnerEffects = CachedBHEResources.instance.spawnerEffects.Values.ToArray();
        for (int i = 0; i < spawnerEffects.Length; i++)
        {
            //Wish I didn't have to use GetComponent
            spawnerEffectsScrollSection.elements[i].GetComponent<SpawnerEffectInfoPanel>().Init(spawnerEffects[i]);
        }

        //Generate Spawner Pattern Buttons
        //  Generate active list
        GenerateActivePatternScrollView();
        //  Generate avaliable list
        availableProjectilePatternsScrollSection.SetNumElements(CachedBHEResources.instance.projectilePatterns.Count);
        string[] availablePatterns = CachedBHEResources.instance.projectilePatterns.Keys.ToArray();
        for(int i = 0; i < availablePatterns.Length; i++)
        {
            //Wish I didn't have to use GetComponent
            availableProjectilePatternsScrollSection.elements[i].GetComponent<ProjectilePatternInfoPanel>().Init(availablePatterns[i]);
        }
    }

    public void SwapPanel(int panel)
    {
        DisableAllPanels();

        switch ((ModTesterPanels)panel)
        {
            case ModTesterPanels.CONTROLS:
                controlsPanel.SetActive(true);
                break;

            case ModTesterPanels.PROJTYPE:
                projTypePanel.SetActive(true);
                break;

            case ModTesterPanels.PROJEFFECTS:
                projEffectsPanel.SetActive(true);
                break;

            case ModTesterPanels.SPAWNEREFFECTS:
                spawnerEffectsPanel.SetActive(true);
                break;

            case ModTesterPanels.PROJECTILEPATTERNS:
                projectilePatternsPanel.SetActive(true);
                break;

            default:
                introPanel.SetActive(true);
                break;
        }
    }

    public void DisableAllPanels()
    {
        introPanel.SetActive(false);
        controlsPanel.SetActive(false);
        projTypePanel.SetActive(false);
        projEffectsPanel.SetActive(false);
        spawnerEffectsPanel.SetActive(false);
        projectilePatternsPanel.SetActive(false);
    }

    public void UpdateProjectileName(string name)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        curProjTypeText.text = "Cur Type: " + name;

        spawner.spawnerProjectileType = name;
    }

    public void AddProjectileEffect(string name)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        spawner.defaultProjectileEffects.Add(name);
    }

    public void RemoveProjectileEffect(string name)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        spawner.defaultProjectileEffects.Remove(name);
    }

    public void AddSpawnerEffect(string name)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        spawner.defaultSpawnerEffects.Add(name);
    }

    public void RemoveSpawnerEffect(string name)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        spawner.defaultSpawnerEffects.Remove(name);
    }

    public void GenerateActivePatternScrollView()
    {
        activeProjectilePatternsScrollSection.SetNumElements(activePatterns.Count);
        for (int i = 0; i < activePatterns.Count; i++)
        {
            //Wish I didn't have to use GetComponent
            activeProjectilePatternsScrollSection.elements[i].GetComponent<ProjectilePatternInfoPanel>().Init(activePatterns[i], i);
        }
    }

    public void AddProjectilePattern(string patternName)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        activePatterns.Add(patternName);
        spawner.defaultProjectilePatterns = activePatterns;

        //This must have at least 2 patterns to be used, and when adding, this will be the case (bc we can't go below 1)!
        randomizePatternsToggle.interactable = true;

        GenerateActivePatternScrollView();
    }

    public void RemoveProjectilePattern(int index)
    {
        if(activePatterns.Count > 1)
        {
            spawner.Disable();
            isSpawnerEnabled = false;
            isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

            activePatterns.RemoveAt(index);
            spawner.defaultProjectilePatterns = activePatterns;

            //If we're down to the last pattern, disable randomization
            if (activePatterns.Count == 1)
            {
                //Change UI
                randomizePatterns = false;
                randomizePatternsToggle.SetIsOnWithoutNotify(false);
                randomizePatternsToggle.interactable = false;

                noRandomRepeats = false;
                noRandomRepeatsToggle.SetIsOnWithoutNotify(false);
                noRandomRepeatsToggle.interactable = false;

                //Change spawner behavior
                spawner.randomizePatterns = false;
                spawner.noRandomRepeats = false;
            }


            GenerateActivePatternScrollView();
        }
    }

    public void ToggleRandomizePatterns()
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);


        if (randomizePatterns)
        {
            randomizePatterns = false;
            randomizePatternsToggle.SetIsOnWithoutNotify(false);

            noRandomRepeats = false;
            noRandomRepeatsToggle.SetIsOnWithoutNotify(false);
            noRandomRepeatsToggle.interactable = false;

            spawner.randomizePatterns = false;
            spawner.noRandomRepeats = false;
        }
        else
        {
            randomizePatterns = true;
            randomizePatternsToggle.SetIsOnWithoutNotify(true);

            noRandomRepeatsToggle.interactable = true;

            spawner.randomizePatterns = true;
        }
    }

    public void ToggleNoRandomRepeats()
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);


        noRandomRepeats = !noRandomRepeats;
        noRandomRepeatsToggle.SetIsOnWithoutNotify(noRandomRepeats);

        spawner.noRandomRepeats = noRandomRepeats;
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

    private void ZoomIn()
    {
        Camera.main.orthographicSize = Camera.main.orthographicSize / 2;

        if(Camera.main.orthographicSize < minFOV)
        {
            Camera.main.orthographicSize = minFOV;
        }
    }

    private void ZoomOut()
    {
        Camera.main.orthographicSize = Camera.main.orthographicSize * 2;

        if (Camera.main.orthographicSize > maxFOV)
        {
            Camera.main.orthographicSize = maxFOV;
        }
    }
}
