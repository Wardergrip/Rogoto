using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveCounterDisplay : MonoBehaviour
{
    [SerializeField] private string _prefix = string.Empty;
    [SerializeField] private TMPro.TextMeshProUGUI _tmpro;
    [SerializeField] private string _suffix = string.Empty;

    private void Start()
    {
        GameSystem.OnWaveStarted += UpdateText;
        UpdateText();
    }

    private void OnDestroy()
    {
        GameSystem.OnWaveStarted -= UpdateText;
    }

    private void UpdateText()
    {
        _tmpro.text = $"{_prefix}{GameSystem.Instance.WaveCounter}{_suffix}";
    }
}
