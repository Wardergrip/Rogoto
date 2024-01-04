using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CommonRepeater : MonoBehaviour
{
    [SerializeField] private float _timeBetweenevents = 1.0f;

    public UnityEvent<GameObject> OnTick;

    private void Awake()
    {
        StartCoroutine(TickCoroutine());
    }

    private IEnumerator TickCoroutine()
    {
        while (true) 
        {
            yield return new WaitForSeconds(_timeBetweenevents);
            OnTick?.Invoke(gameObject);
        }
    }
}
