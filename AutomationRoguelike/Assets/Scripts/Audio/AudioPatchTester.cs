using UnityEngine;

public class AudioPatchTester : MonoBehaviour
{
    enum PlayMethod { Play, PlayOneShot, PlayTrailing2D, PlayOneShotTrailing2D, PlayTrailing3D, PlayOneShotTrailing3D }

    [SerializeField] private bool _play = false;
    [SerializeField] private AudioPatch _patch;
    [SerializeField] private PlayMethod _playMethod;

    [Header("Optional")]
    [SerializeField] private AudioSource _source;
    [SerializeField] private AudioSource _trailAudioSourcePrefab;

    private void Update()
    {
        if (_play == false) return;

        _play = false;
        if (_source == null)
        {
            _source = gameObject.AddComponent<AudioSource>();
        }
        if (_trailAudioSourcePrefab == null)
        {
            GameObject obj = new GameObject("[THAT] TESTING TrailAudioSourcePrefab");
            obj.transform.parent = transform;
            _trailAudioSourcePrefab = obj.AddComponent<AudioSource>();
        }
        if (_patch == null)
        {
            Debug.LogWarning("No audio patch assigned");
            return;
        }

        switch (_playMethod)
        {
            case PlayMethod.Play:
                _patch.Play(_source);
                break;
            case PlayMethod.PlayOneShot:
                _patch.PlayOneShot(_source);
                break;
            case PlayMethod.PlayTrailing2D:
                _patch.PlayTrailing2D(/*_trailAudioSourcePrefab.*/transform);
                break;
            case PlayMethod.PlayOneShotTrailing2D:
                _patch.PlayOneShotTrailing2D(/*_trailAudioSourcePrefab.*/transform);
                break;
            case PlayMethod.PlayTrailing3D:
                _patch.PlayTrailing3D(/*_trailAudioSourcePrefab.*/transform);
                break;
            case PlayMethod.PlayOneShotTrailing3D:
                _patch.PlayOneShotTrailing3D(/*_trailAudioSourcePrefab.*/transform);
                break;
            default:
                Debug.LogWarning("This play method is not supported. Please add it to the switch case.");
                break;
        }
    }
}