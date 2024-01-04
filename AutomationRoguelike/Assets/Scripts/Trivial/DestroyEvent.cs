using UnityEngine;
using UnityEngine.Events;

public class DestroyEvent : MonoBehaviour
{
    public UnityEvent<GameObject> OnDestroyEvent;

    private void OnDestroy()
    {
        OnDestroyEvent?.Invoke(gameObject);
    }
}
