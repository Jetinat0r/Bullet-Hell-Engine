using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BehaviorManagerExtraInspector : EditorWindow
{
    public string lastBehaviorDump = "";

    [MenuItem("Window/Custom/Behavior Cache Watcher")]
    public static void ShowWindow()
    {
        GetWindow<BehaviorManagerExtraInspector>("Behavior Cache Watcher");
    }

    private void OnGUI()
    {
        GUILayout.Label("Selected Behavior Manager", EditorStyles.boldLabel);

        BehaviorManager[] behaviorManagers = Selection.GetFiltered<BehaviorManager>(SelectionMode.Deep);
        
        if(behaviorManagers.Length > 0)
        {
            if (behaviorManagers.Length > 1)
            {
                GUILayout.TextArea("You should never have more than one Behavior Manager!");
                return;
            }

            BehaviorManager behaviorManager = behaviorManagers[0];

            GUILayout.Label($"{behaviorManager.name}", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Get Dump"))
            {
                string dump = "";

                #region Get Proj Behaviors
                int totalProjBehaviors = 0;
                string detailedActiveProjBehaviors = "";
                foreach (string projBehaviorKey in behaviorManager.activeEntityBehaviors.Keys)
                {
                    List<EntityBehaviour> projBehaviorList = behaviorManager.activeEntityBehaviors[projBehaviorKey];
                    totalProjBehaviors += projBehaviorList.Count;

                    detailedActiveProjBehaviors += projBehaviorKey + ": " + projBehaviorList.Count + "\n";
                }

                string detailedInactiveProjBehaviors = "";
                foreach (string projBehaviorKey in behaviorManager.inactiveEntityBehaviors.Keys)
                {
                    List<EntityBehaviour> projBehaviorList = behaviorManager.inactiveEntityBehaviors[projBehaviorKey];
                    totalProjBehaviors += projBehaviorList.Count;

                    detailedInactiveProjBehaviors += projBehaviorKey + ": " + projBehaviorList.Count + "\n";
                }
                dump += "Total Entity Behaviors: " + totalProjBehaviors + "\n";
                #endregion

                #region Get Spawner Behaviors
                int totalSpawnerBehaviors = 0;
                string detailedActiveSpawnerBehaviors = "";
                foreach (string spawnerBehaviorKey in behaviorManager.activeSpawnerBehaviors.Keys)
                {
                    List<SpawnerBehavior> spawnerBehaviorList = behaviorManager.activeSpawnerBehaviors[spawnerBehaviorKey];
                    totalSpawnerBehaviors += spawnerBehaviorList.Count;

                    detailedActiveSpawnerBehaviors += spawnerBehaviorKey + ": " + spawnerBehaviorList.Count + "\n";
                }

                string detailedInactiveSpawnerBehaviors = "";
                foreach (string spawnerBehaviorKey in behaviorManager.inactiveSpawnerBehaviors.Keys)
                {
                    List<SpawnerBehavior> spawnerBehaviorList = behaviorManager.inactiveSpawnerBehaviors[spawnerBehaviorKey];
                    totalSpawnerBehaviors += spawnerBehaviorList.Count;

                    detailedInactiveSpawnerBehaviors += spawnerBehaviorKey + ": " + spawnerBehaviorList.Count + "\n";
                }
                dump += "Total Spawner Behaviors: " + totalSpawnerBehaviors + "\n";
                #endregion

                dump += "\n"
                    + "=== Active Entity Behaviors ===" + "\n"
                    + detailedActiveProjBehaviors + "\n"
                    + "=== Inactive Entity Behaviors ===" + "\n"
                    + detailedInactiveProjBehaviors + "\n"
                    + "=== Active Spawner Behaviors ===" + "\n"
                    + detailedActiveSpawnerBehaviors + "\n"
                    + "=== Inactive Spawner Behaviors ===" + "\n"
                    + detailedInactiveSpawnerBehaviors;

                lastBehaviorDump = dump;
            }

            GUILayout.TextArea(lastBehaviorDump);
        }
    }
}
