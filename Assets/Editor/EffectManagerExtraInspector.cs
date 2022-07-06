using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EffectManagerExtraInspector : EditorWindow
{
    public string lastEffectDump = "";

    [MenuItem("Window/Custom/Effect Cache Watcher")]
    public static void ShowWindow()
    {
        GetWindow<EffectManagerExtraInspector>("Effect Cache Watcher");
    }

    private void OnGUI()
    {
        GUILayout.Label("Selected Effect Manager", EditorStyles.boldLabel);

        EffectManager[] effectManagers = Selection.GetFiltered<EffectManager>(SelectionMode.Deep);
        
        if(effectManagers.Length > 0)
        {
            if (effectManagers.Length > 1)
            {
                GUILayout.TextArea("You should never have more than one Effect Manager!");
                return;
            }

            EffectManager effectManager = effectManagers[0];

            GUILayout.Label($"{effectManager.name}", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("Get Dump"))
            {
                string dump = "";

                #region Get Proj Effects
                int totalProjEffects = 0;
                string detailedActiveProjEffects = "";
                foreach (string projEffectKey in effectManager.activeProjectileEffects.Keys)
                {
                    List<ProjectileEffect> projEffectList = effectManager.activeProjectileEffects[projEffectKey];
                    totalProjEffects += projEffectList.Count;

                    detailedActiveProjEffects += projEffectKey + ": " + projEffectList.Count + "\n";
                }

                string detailedInactiveProjEffects = "";
                foreach (string projEffectKey in effectManager.inactiveProjectileEffects.Keys)
                {
                    List<ProjectileEffect> projEffectList = effectManager.inactiveProjectileEffects[projEffectKey];
                    totalProjEffects += projEffectList.Count;

                    detailedInactiveProjEffects += projEffectKey + ": " + projEffectList.Count + "\n";
                }
                dump += "Total Projectile Effects: " + totalProjEffects + "\n";
                #endregion

                #region Get Spawner Effects
                int totalSpawnerEffects = 0;
                string detailedActiveSpawnerEffects = "";
                foreach (string spawnerEffectKey in effectManager.activeSpawnerEffects.Keys)
                {
                    List<SpawnerEffect> spawnerEffectList = effectManager.activeSpawnerEffects[spawnerEffectKey];
                    totalSpawnerEffects += spawnerEffectList.Count;

                    detailedActiveSpawnerEffects += spawnerEffectKey + ": " + spawnerEffectList.Count + "\n";
                }

                string detailedInactiveSpawnerEffects = "";
                foreach (string spawnerEffectKey in effectManager.inactiveSpawnerEffects.Keys)
                {
                    List<SpawnerEffect> spawnerEffectList = effectManager.inactiveSpawnerEffects[spawnerEffectKey];
                    totalSpawnerEffects += spawnerEffectList.Count;

                    detailedInactiveSpawnerEffects += spawnerEffectKey + ": " + spawnerEffectList.Count + "\n";
                }
                dump += "Total Spawner Effects: " + totalSpawnerEffects + "\n";
                #endregion

                dump += "\n"
                    + "=== Active Projectile Effects ===" + "\n"
                    + detailedActiveProjEffects + "\n"
                    + "=== Inactive Projectile Effects ===" + "\n"
                    + detailedInactiveProjEffects + "\n"
                    + "=== Active Spawner Effects ===" + "\n"
                    + detailedActiveSpawnerEffects + "\n"
                    + "=== Inactive Spawner Effects ===" + "\n"
                    + detailedInactiveSpawnerEffects;

                lastEffectDump = dump;
            }

            GUILayout.TextArea(lastEffectDump);
        }
    }
}
