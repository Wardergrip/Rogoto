using UnityEngine;

public class EffectTester : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Effect _effect;
    [SerializeField] private EffectRarity _effectRarity;
    [SerializeField] private KeyCode _key =  KeyCode.L;
    void Update()
    {
        if (Input.GetKeyDown(_key))
        {
            if (!_effect.IsAvailable())
            {
                Debug.LogWarning($"Effect {_effect} is not available");
                return;
            }
            _effect.Init(_effectRarity);
            _effect.Excecute();
        }
    }
#endif
}
