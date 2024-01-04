using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Trivial
{
    public class RandomTick : MonoBehaviour
    {
        [SerializeField] private float _minWaitTimeSeconds = 0.5f;
        [SerializeField] private float _maxWaitTimeSeconds = 1.0f;
        [Tooltip("If true, uses WaitForSecondsRealtime instead of WaitForSeconds. WaitForSeconds uses scaled time which can be used to slow game down or make it faster.")]
        [SerializeField] private bool _waitRealTime = false;
        public UnityEvent OnTick;

        private void Awake()
        {
            StartCoroutine(TickCoroutine());
        }

        private IEnumerator TickCoroutine()
        {
            float waitInterval;
            while (true)
            {
                waitInterval = Random.Range(_minWaitTimeSeconds, _maxWaitTimeSeconds);
                yield return _waitRealTime ? new WaitForSecondsRealtime(waitInterval) : new WaitForSeconds(waitInterval);
                OnTick?.Invoke();
            }
        }
    }
}