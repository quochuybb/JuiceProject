using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

public class LogConsoleUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI logText;
    
    [Header("Settings")]
    [SerializeField] private int maxLines = 50; 
    [SerializeField] private bool showLogs = true; 

    private StringBuilder logBuilder = new StringBuilder();
    private int currentLines = 0;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLogMessage;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLogMessage;
    }

    private void HandleLogMessage(string logString, string stackTrace, LogType type)
    {
        if (!showLogs || logText == null) return;

        string colorTag = "#FFFFFF"; 
        if (type == LogType.Warning) colorTag = "#FFFF00"; 
        else if (type == LogType.Error || type == LogType.Exception) colorTag = "#FF0000"; // Đỏ cho Error

        string newLog = $"<color={colorTag}>{logString}</color>\n";

        logBuilder.Append(newLog);
        currentLines++;

        if (currentLines > maxLines)
        {
            RemoveOldestLine();
        }

        logText.text = logBuilder.ToString();
    }

    private void RemoveOldestLine()
    {
        int endOfFirstLine = 0;
        for (int i = 0; i < logBuilder.Length; i++)
        {
            if (logBuilder[i] == '\n')
            {
                endOfFirstLine = i + 1; 
                break;
            }
        }

        if (endOfFirstLine > 0)
        {
            logBuilder.Remove(0, endOfFirstLine);
            currentLines--;
        }
    }

    public void ClearLogs()
    {
        logBuilder.Clear();
        currentLines = 0;
        if (logText != null) logText.text = "";
    }
}
