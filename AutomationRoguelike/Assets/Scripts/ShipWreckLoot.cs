using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShipWreckLoot : MonoBehaviour
{
    [SerializeField] private GameObject _rewardIndicator;
    [SerializeField] private Canvas _blueprintCanvas;
    private bool _rewardTaken;
    private bool _locked = true;
    private bool _rewardEarned;
    public UnityEvent OnBlueprintChosen;
    private float _storedGameSpeed;
    private bool _checkOnceSloppy;
    private void Awake()
    {
        GameSystem.OnBuildPhaseStarted += Unlock;
        GameSystem.OnWaveStarted += Lock;
        _rewardIndicator.SetActive(false);
    }
    private void OnDestroy()
    {
        GameSystem.OnBuildPhaseStarted -= Unlock;
        GameSystem.OnWaveStarted -= Lock;

    }
    private void Lock()
    {
        _rewardIndicator.SetActive(false);
        _locked = true;
    }
    private void Unlock()
    {
        if ((GameSystem.Instance.WaveCounter-1) % 2 == 0 && _checkOnceSloppy )
        {
            _rewardEarned = true;
        }
        if (_rewardTaken != true && _rewardEarned&&_checkOnceSloppy)
        {
            _rewardIndicator.SetActive(true);
            _locked = false;
        }
        _checkOnceSloppy = true;
    }
    public void GiveBlueprint()
    {
        if (_rewardTaken||_locked||!_rewardEarned)
            return;
        _storedGameSpeed = GameSystem.Instance.GameSpeed;
        GameSystem.Instance.GameSpeed = 0;
        Instantiate(_blueprintCanvas, Vector3.zero,Quaternion.identity,transform);
        _rewardTaken = true;
        _rewardIndicator.SetActive(false);
    }
    public void BlueprintChosen()
    {
        GameSystem.Instance.GameSpeed = _storedGameSpeed;
        OnBlueprintChosen.Invoke();
        Destroy(gameObject);
    }
}
