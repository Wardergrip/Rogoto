using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Turret),typeof(AttackSpeedModifier))]
public class BoosterTurret : MonoBehaviour
{
    private class BoosterTurretEffectIdentifier : MonoBehaviour {}

    [SerializeField] private Turret _turret;
    [SerializeField] private AttackSpeedModifier _attackSpeedModifier;
    private List<TimeBetweenAttacks> _timebetweenAttacks = new();
    private float _previouslyApplied = 1.0f;
    [SerializeField] private GameObject _boostEffectRFX;

    private void Awake()
    {
        if (_turret == null) _turret = GetComponent<Turret>();
        if (_attackSpeedModifier == null) _attackSpeedModifier = GetComponent<AttackSpeedModifier>();


        // Events
        _turret.OnTurretSpawnedInRange += Turret_OnTurretSpawnedInRange;
        _turret.OnTurretDestroyedInRange += Turret_OnTurretDestroyedInRange;
        _turret.OnTierChanged.AddListener(TierChanged);
    }

    private void OnDestroy()
    {
        _turret.OnTurretSpawnedInRange -= Turret_OnTurretSpawnedInRange;
        _turret.OnTurretDestroyedInRange -= Turret_OnTurretDestroyedInRange;
        _turret.OnTierChanged.RemoveListener(TierChanged);

        foreach (TimeBetweenAttacks timeBetweenAttacks in _timebetweenAttacks)
        {
            RemoveBoost(timeBetweenAttacks);
        }
    }

    private void Start()
    {
        foreach (GameObject spawnedObj in _turret.SpawnObjectsFromImage.SpawnedObjects)
        {
            if (!spawnedObj.TryGetComponent(out BoxCollider coll)) continue;
            //Debug.DrawRay(coll.transform.position + new Vector3(0, 1, 0), Vector3.down, Color.red, 10);
            if (Physics.Raycast(coll.transform.position + new Vector3(0, 1, 0), Vector3.down,out RaycastHit hitInfo))
            {
                if (hitInfo.collider.gameObject.TryGetComponent(out TimeBetweenAttacks timeBetweenAttacks))
                {
                    ApplyBoost(timeBetweenAttacks);
                    _timebetweenAttacks.Add(timeBetweenAttacks);
                }
            }
        }
        _previouslyApplied = AttackSpeedModFinalFinal;
    }

    private float AttackSpeedModFinalFinal
    {
        get => (1 + _attackSpeedModifier.FinalValue);
	}

    private void UpdateAllTurretsToBoost()
    {
        foreach (TimeBetweenAttacks timeBetweenAttacks in _timebetweenAttacks)
        {
            timeBetweenAttacks.FactorBonusValue *= _previouslyApplied;
            timeBetweenAttacks.FactorBonusValue /= AttackSpeedModFinalFinal;
			timeBetweenAttacks.ForceRefreshVisual();
		}
        _previouslyApplied = _attackSpeedModifier.FinalValue;
    }

    private void ApplyBoost(TimeBetweenAttacks timeBetweenAttacks)
    {
        timeBetweenAttacks.FactorBonusValue /= AttackSpeedModFinalFinal;
        timeBetweenAttacks.ForceRefreshVisual();
        if (_boostEffectRFX != null)
        {
			Instantiate(_boostEffectRFX, timeBetweenAttacks.transform).AddComponent<BoosterTurretEffectIdentifier>();
		}
    }

    private void RemoveBoost(TimeBetweenAttacks timeBetweenAttacks)
    {
        timeBetweenAttacks.FactorBonusValue *= _previouslyApplied;
		timeBetweenAttacks.ForceRefreshVisual();
        foreach (BoosterTurretEffectIdentifier effectIdentifier in timeBetweenAttacks.gameObject.GetComponentsInChildren<BoosterTurretEffectIdentifier>())
        {
            Destroy(effectIdentifier.gameObject);
        };
	}

    private void TierChanged()
    {
        UpdateAllTurretsToBoost();
    }

    private void Turret_OnTurretSpawnedInRange(Turret turretThatSpawned)
    {
        if (!turretThatSpawned.TryGetComponent(out TimeBetweenAttacks timeBetweenAttacks)) return;
        _timebetweenAttacks.Add(timeBetweenAttacks);
        ApplyBoost(timeBetweenAttacks);
    }

    private void Turret_OnTurretDestroyedInRange(Turret turretBeingDestroyed)
    {
        if (!turretBeingDestroyed.TryGetComponent(out TimeBetweenAttacks timeBetweenAttacks)) return;
        _timebetweenAttacks.Remove(timeBetweenAttacks);
    }
}
