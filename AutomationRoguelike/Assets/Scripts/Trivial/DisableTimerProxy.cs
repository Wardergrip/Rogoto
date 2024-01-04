using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DisableTimerProxy : MonoBehaviour
{
    [SerializeField] private float _timeUntillDisableInSeconds;
    public UnityEvent OnDisableEvent;
    private void OnEnable()
    {
        StartCoroutine(DisableAfterTime());
    }
    private IEnumerator DisableAfterTime()
    {
       yield return new WaitForSeconds(_timeUntillDisableInSeconds);
        OnDisableEvent.Invoke();
        this.gameObject.SetActive(false);
    }
}
