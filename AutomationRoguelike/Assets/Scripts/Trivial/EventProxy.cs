using UnityEngine;
using UnityEngine.Events;

public class EventProxy : MonoBehaviour
{
    public UnityEvent OnEvent;
    public void TriggerEvent()
    {
            OnEvent.Invoke();
    }
}
