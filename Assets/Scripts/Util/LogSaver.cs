using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Text;

//This saves the debug log to a .txt file
public class LogSaver : MonoBehaviour
{
    public static LogSaver instance;
    private StringBuilder stringBuilder = new StringBuilder();

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

    private void HandleLog(string logString, string stackTrace, LogType logType)
    {
        if (FilterLog(logString))
        {
            stringBuilder.Append($"[{logType}] {logString}" + '\n' +
                $"{stackTrace}" + '\n');
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

    
}
