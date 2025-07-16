using UnityEngine;

public class UnityDebugLogger : ILogger
{
    public void Log(string message)
    {
        Debug.Log($"[Log] {message}");
    }

    public void LogError(string message)
    {
        Debug.LogError($"[Error] {message}");
    }

    public void LogWarning(string message)
    {
        Debug.LogWarning($"[Warning] {message}");
    }
}