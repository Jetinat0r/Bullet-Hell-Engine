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
    private EntitySpawner spawner;

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

    [Header("Entity Prefab Panel")]
    //Panel controlling the current type of entity being used
    [SerializeField]
    private GameObject projTypePanel;
    [SerializeField]
    private TMP_Text curProjTypeText;
    [SerializeField]
    private ScrollPopulator projTypeScrollSection;

    [Header("Entity Behaviors Panel")]
    //Panel controlling what behaviors are applied to the entities spawned
    [SerializeField]
    private GameObject projBehaviorsPanel;
    [SerializeField]
    private ScrollPopulator projBehaviorsScrollSection;

    [Header("Spawner Behaviors Panel")]
    //Panel controlling what behaviors are applied to the spawner
    [SerializeField]
    private GameObject spawnerBehaviorsPanel;
    [SerializeField]
    private ScrollPopulator spawnerBehaviorsScrollSection;

    [Header("Entity Patterns Panel")]
    //Panel controlling what patterns are used and in what order
    [SerializeField]
    private GameObject spawnerPatternsPanel;
    [SerializeField]
    private ScrollPopulator activeSpawnerPatternsScrollSection;
    [SerializeField]
    private ScrollPopulator availableSpawnerPatternsScrollSection;
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
        //Generate Entity Type Buttons
        projTypeScrollSection.SetNumElements(CachedBHEResources.instance.entityPrefabs.Count);
        string[] entityPrefabNames = CachedBHEResources.instance.entityPrefabs.Keys.ToArray();
        for(int i = 0; i < entityPrefabNames.Length; i++)
        {
            //Wish I didn't have to use GetComponent
            projTypeScrollSection.elements[i].GetComponent<EntityPrefabInfoPanel>().Init(entityPrefabNames[i]);
        }

        //Generate Entity Behavior Buttons
        projBehaviorsScrollSection.SetNumElements(CachedBHEResources.instance.entityBehaviors.Count);
        EntityBehaviour[] entityBehaviors = CachedBHEResources.instance.entityBehaviors.Values.ToArray();
        for (int i = 0; i < entityBehaviors.Length; i++)
        {
            //Wish I didn't have to use GetComponent
            projBehaviorsScrollSection.elements[i].GetComponent<EntityBehaviorInfoPanel>().Init(entityBehaviors[i]);
        }

        //Generate Spawner Behavior Buttons
        spawnerBehaviorsScrollSection.SetNumElements(CachedBHEResources.instance.spawnerBehaviors.Count);
        SpawnerBehavior[] spawnerBehaviors = CachedBHEResources.instance.spawnerBehaviors.Values.ToArray();
        for (int i = 0; i < spawnerBehaviors.Length; i++)
        {
            //Wish I didn't have to use GetComponent
            spawnerBehaviorsScrollSection.elements[i].GetComponent<SpawnerBehaviorInfoPanel>().Init(spawnerBehaviors[i]);
        }

        //Generate Spawner Pattern Buttons
        //  Generate active list
        GenerateActivePatternScrollView();
        //  Generate avaliable list
        availableSpawnerPatternsScrollSection.SetNumElements(CachedBHEResources.instance.spawnerPatterns.Count);
        string[] availablePatterns = CachedBHEResources.instance.spawnerPatterns.Keys.ToArray();
        for(int i = 0; i < availablePatterns.Length; i++)
        {
            //Wish I didn't have to use GetComponent
            availableSpawnerPatternsScrollSection.elements[i].GetComponent<SpawnerPatternInfoPanel>().Init(availablePatterns[i]);
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
                projBehaviorsPanel.SetActive(true);
                break;

            case ModTesterPanels.SPAWNEREFFECTS:
                spawnerBehaviorsPanel.SetActive(true);
                break;

            case ModTesterPanels.PROJECTILEPATTERNS:
                spawnerPatternsPanel.SetActive(true);
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
        projBehaviorsPanel.SetActive(false);
        spawnerBehaviorsPanel.SetActive(false);
        spawnerPatternsPanel.SetActive(false);
    }

    public void UpdateEntityName(string name)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        curProjTypeText.text = "Cur Type: " + name;

        spawner.spawnerEntityType = name;
    }

    public void AddEntityBehavior(string name)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        spawner.defaultEntityBehaviors.Add(name);
    }

    public void RemoveEntityBehavior(string name)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        spawner.defaultEntityBehaviors.Remove(name);
    }

    public void AddSpawnerBehavior(string name)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        spawner.defaultSpawnerBehaviors.Add(name);
    }

    public void RemoveSpawnerBehavior(string name)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        spawner.defaultSpawnerBehaviors.Remove(name);
    }

    public void GenerateActivePatternScrollView()
    {
        activeSpawnerPatternsScrollSection.SetNumElements(activePatterns.Count);
        for (int i = 0; i < activePatterns.Count; i++)
        {
            //Wish I didn't have to use GetComponent
            activeSpawnerPatternsScrollSection.elements[i].GetComponent<SpawnerPatternInfoPanel>().Init(activePatterns[i], i);
        }
    }

    public void AddSpawnerPattern(string patternName)
    {
        spawner.Disable();
        isSpawnerEnabled = false;
        isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

        activePatterns.Add(patternName);
        spawner.defaultSpawnerPatterns = activePatterns;

        //This must have at least 2 patterns to be used, and when adding, this will be the case (bc we can't go below 1)!
        randomizePatternsToggle.interactable = true;

        GenerateActivePatternScrollView();
    }

    public void RemoveSpawnerPattern(int index)
    {
        if(activePatterns.Count > 1)
        {
            spawner.Disable();
            isSpawnerEnabled = false;
            isSpawnerEnabledToggle.SetIsOnWithoutNotify(false);

            activePatterns.RemoveAt(index);
            spawner.defaultSpawnerPatterns = activePatterns;

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

    public void ClearEntities()
    {
        EntityManager.instance.ClearEntityPools();
    }

    public void ClearBehaviorPools()
    {
        BehaviorManager.instance.ClearAllBehaviorPools();
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
