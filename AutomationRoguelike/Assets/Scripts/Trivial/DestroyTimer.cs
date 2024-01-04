using UnityEngine;

namespace Trivial
{
    public class DestroyTimer : MonoBehaviour
    {
        [SerializeField] private float _time = 1f;
        [SerializeField] private bool _runOnStart = false;
        public bool RunOnStart { get => _runOnStart; }

        /// <summary>
        /// Time can only have meaningful change when it is set before the Start of DestroyTimer is called.
        /// <para>Start is called the first frame that the script is active.</para>
        /// <para>If you want to set the time later, disable the script.</para>
        /// </summary>
        public float Time { get { return _time; } set { _time = value; } }

        void Start()
        {
            if (RunOnStart) StartTimer();
        }
        
        public void StartTimer()
        {
            Destroy(gameObject, _time);
        }
    }
}