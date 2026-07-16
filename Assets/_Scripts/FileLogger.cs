using UnityEngine;
using System.IO;

public class FileLogger : MonoBehaviour
{
    string logPath;


    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        logPath = Path.Combine(
            Application.persistentDataPath,
            "app_log.txt"
        );

        Debug.Log("[LOGGER] Log file: " + logPath);

        Application.logMessageReceived += HandleLog;
    }

    void HandleLog(string condition, string stackTrace, LogType type)
    {
        File.AppendAllText(
            logPath,
            $"[{type}] {condition}\n"
        );
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
}