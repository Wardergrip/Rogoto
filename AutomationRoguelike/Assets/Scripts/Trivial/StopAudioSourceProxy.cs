using UnityEngine;

namespace Trivial
{
    public class StopAudioSourceProxy : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;

        public void Stop()
        {
            _audioSource.Stop();
        }

        public void Stop(AudioSource audioSource)
        {
            audioSource.Stop();
        }
    }
}