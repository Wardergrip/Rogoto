using UnityEngine;
using UnityEngine.Audio;

public class MixerVolumeProxy : MonoBehaviour
{
    [SerializeField] private AudioMixer _mixer;
    [SerializeField] private string VolumeParameterName;

    public void SetVolumePercentage(float volume)
    {
        _mixer.SetFloat(VolumeParameterName, Mathf.Log10(volume) * 20);
    }
}
