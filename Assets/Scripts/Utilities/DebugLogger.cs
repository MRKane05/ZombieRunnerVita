using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DebugLogger : MonoBehaviour {
    public TMP_Text DebugLogText;
    public string TextLog = "";

    void Awake()
    {
        Application.logMessageReceived += LogCallback;
    }

    void OnEnable()
    {
        Application.logMessageReceived += LogCallback;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= LogCallback;
    }

    void LogCallback(string logString, string stackTrace, LogType type)
    {
        //if ((type == LogType.Warning || type == LogType.Error) && !logString.Contains("UnityWaterMark"))
        //{
            TextLog += '\n' + logString;
        //}
        DebugLogText.text = TextLog;
        //Or Append the log to the old one
        //logText.text += logString + "\r\n";
    }
}
