using UnityEngine;

public class DebugProxy : MonoBehaviour
{
    public void DebugLog(string message) => Debug.Log(message);
    public void DebugLog() => DebugLog($"DebugProxy triggered on {gameObject}");
}
