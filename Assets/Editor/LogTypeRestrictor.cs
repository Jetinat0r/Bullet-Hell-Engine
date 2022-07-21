using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEditor.AnimatedValues;
//using UnityEditor;

using System.Reflection;

//I made this specifically to stop getting spammed with "No script asset for [Mod Scriptable Object]..."
public class LogTypeRestrictor : EditorWindow
{
    /* === Log Type Codes ===
     * 0       - All the user-generated logs (info, warning, error, etc.)
     * 2       - Errors reported by the editor 
     * 256     - User-generated errors, exceptions, etc
     * 512     - all warnings (user-generated and *logged by the editor itself*)
     * 262144  - A specific mode that removes only the "No script asset for ..." Not sure if it removes any other warning reported by Unity
     * 8406016 - User-generated info message
     * 8405504 - User-logged warning message
     */
    public int LogType = 0;
    public int CurLogType = 0;

    public bool isFiltering = false;
    public AnimBool DisplayKnownCodes;
    public string KnownCodes =
        "* 0       - All the user-generated logs (info, warning, error, etc.)" + "\n" +
        "* 2       - Errors reported by the editor " + "\n" +
        "* 256     - User-generated errors, exceptions, etc" + "\n" +
        "* 512     - all warnings (user-generated and *logged by the editor itself*)" + "\n" +
        "* 262144  - A specific mode that removes only the \"No script asset for ...\" Not sure if it removes any other warning reported by Unity" + "\n" +
        "* 8406016 - User-generated info message" + "\n" +
        "* 8405504 - User-logged warning message";


    private MethodInfo RemoveLogEntriesByMode;

    [MenuItem("Window/Custom/Log Type Restrictor")]
    public static void ShowWindow()
    {
        GetWindow<LogTypeRestrictor>("Log Type Restrictor");
    }

    private void OnEnable()
    {
        try
        {
            Assembly assembly = Assembly.GetAssembly(typeof(UnityEditor.Editor));
            Type type = assembly.GetType("UnityEditor.LogEntry");
            //                                                              Both flags are necessary
            MethodInfo method = type.GetMethod("RemoveLogEntriesByMode", BindingFlags.Static | BindingFlags.NonPublic);
            RemoveLogEntriesByMode = method;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }

        DisplayKnownCodes = new AnimBool(false);
        DisplayKnownCodes.valueChanged.AddListener(Repaint);
    }

    private void OnGUI()
    {
        DisplayKnownCodes.target = EditorGUILayout.ToggleLeft("Display Known Types", DisplayKnownCodes.target);

        if (EditorGUILayout.BeginFadeGroup(DisplayKnownCodes.faded))
        {
            EditorGUI.indentLevel++;

            GUILayout.TextArea(KnownCodes);

            EditorGUI.indentLevel--;
        }
        EditorGUILayout.EndFadeGroup();


        GUILayout.BeginHorizontal();
        GUILayout.Label("Log Type: ");
        LogType = EditorGUILayout.IntField(LogType);
        GUILayout.EndHorizontal();

        if(GUILayout.Button("Update Log Type"))
        {
            CurLogType = LogType;
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Is Filtering:");
        isFiltering = EditorGUILayout.Toggle(isFiltering);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Cur Log Type: ");
        GUILayout.TextArea(CurLogType.ToString());
        GUILayout.EndHorizontal();
    }

    private void Update()
    {
        if (isFiltering)
        {
            FilterLog();
        }
    }

    private void FilterLog()
    {
        try
        {
            RemoveLogEntriesByMode.Invoke(this, new object[] { LogType });
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
