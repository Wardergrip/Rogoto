using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class MatchEndWithReload : MonoBehaviour
{
    [SerializeField] private TimeBetweenAttacks _timeBetweenAttacks;
    [SerializeField] private AudioPatch _patch;
    public UnityEvent OnShouldPlay;

    public void StartTimer()
    {
        StartCoroutine(TimerCoroutine());
    }

    private IEnumerator TimerCoroutine()
    {
        yield return new WaitForSeconds(_timeBetweenAttacks.FinalValue - _patch.LongestClipTime);
        OnShouldPlay?.Invoke();
    }
}
