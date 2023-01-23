using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;
using TMPro;
using UnityEngine.UI;

//This saves the debug log to a .txt file
public class LogSaver : MonoBehaviour
{
    public static LogSaver instance;
    private StringBuilder stringBuilder = new StringBuilder();

    [Header("In Game Logger")]
    [SerializeField]
    private RectTransform debugLogPanel;
    [SerializeField]
    private ScrollRect debugLogScrollController;
    [SerializeField]
    private TMP_Text inGameLog;
    private enum DebugLogPanelState
    {
        CLOSED = 0,
        MAXIMIZED = 1,
        MINI = 2,
    }
    private DebugLogPanelState debugLogPanelState = DebugLogPanelState.CLOSED;

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

        Application.logMessageReceived += HandleLog;
        //Application.logMessageReceivedThreaded += HandleLog;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Tilde))
        {
            ChangeDebugLogPanelState();
        }
    }

    private void HandleLog(string logString, string stackTrace, LogType logType)
    {
        if (FilterLog(logString))
        {
            stringBuilder.Append($"[{logType}] {logString}" + '\n' +
                $"{stackTrace}" + '\n');

            UpdateInGameLog(logString, logType, stackTrace);
        }
    }

    //TODO: Convert this to an ILogHandler.
    //
    //Returns false if the log string should not be added to the final output log, and true if it should
    //Currently checks for:
    // - The log warning for scriptable objects that don't have script objects (often caused by mods)
    private bool FilterLog(string logString)
    {
        if(logString.StartsWith("No script asset for") && logString.EndsWith("Check that the definition is in a file of the same name."))
        {
            return false;
        }

        return true;
    }


    public void SaveLogToFile()
    {
        File.WriteAllText(Application.persistentDataPath + "/DebugLog.txt", stringBuilder.ToString());
        Debug.Log("Log Saved!");
    }

    private void ChangeDebugLogPanelState()
    {
        switch (debugLogPanelState)
        {
            case DebugLogPanelState.CLOSED:
                debugLogPanelState = DebugLogPanelState.MAXIMIZED;
                debugLogPanel.offsetMax = new Vector2(0, 0);

                debugLogPanel.gameObject.SetActive(true);
                break;

            case DebugLogPanelState.MAXIMIZED:
                debugLogPanelState = DebugLogPanelState.MINI;
                debugLogPanel.offsetMax = new Vector2(-(3*Screen.width) / 4, -(2*Screen.height) / 3);
                //debugLogPanel.sizeDelta = new Vector2(Screen.width / 4, Screen.height / 3);

                debugLogPanel.gameObject.SetActive(true);
                break;

            case DebugLogPanelState.MINI:
                debugLogPanelState = DebugLogPanelState.CLOSED;

                debugLogPanel.gameObject.SetActive(false);
                break;

            default:
                Debug.LogError("Invalid Debug Log Panel State");
                break;
        }
    }

    private void UpdateInGameLog(string logString, LogType logType, string stackTrace = "")
    {
        //newline weirdness is to make the console look cleaner
        inGameLog.text += '\n';
        switch (logType)
        {
            case LogType.Log:
                inGameLog.text += "<color=white>";
                break;

            case LogType.Warning:
                inGameLog.text += "<color=yellow>";
                
                break;

            case LogType.Error:
                inGameLog.text += "<color=red>";
                break;

            case LogType.Assert:
            case LogType.Exception:
            default:
                inGameLog.text += "<color=orange>";
                break;
        }

        inGameLog.text += $"[{logType}] {logString}";
        if (stackTrace != "")
        {
            inGameLog.text += '\n' + stackTrace;
        }

        inGameLog.text += "</color>";
    }
}
