using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AttackSpeedOnKill : MonoBehaviour
{
	private List<Turret> _turrets = new();
	private TimeBetweenAttacks _timeBetweenAttacks;
	private bool _isBoostActive = false;
	private float _initialFactorBonusValue;
	public float EffectLength { get; set; } = 1;
	public OverdriveSpeedMultiplier SpeedMultiplier { get; set; }

	public UnityEvent OnGainAttackSpeed;
	public UnityEvent OnLoseAttackSpeed;

	private void Awake()
	{
		_turrets = GetComponents<Turret>().ToList();
		_turrets[0].gameObject.TryGetComponent(out TimeBetweenAttacks timeBetweenAttacks);
		_timeBetweenAttacks = timeBetweenAttacks;
		foreach (var turret in _turrets)
			turret.OnProjectileKill += AttackSpeedBoost;
	}

	private void AttackSpeedBoost(ProjectileHitData projectileHitData)
	{
		if (!_isBoostActive)
		{
			StartCoroutine(ApplyAttackSpeedBoost());
		}
		else
		{
			ResetBoostTimer();
		}
	}

	private IEnumerator ApplyAttackSpeedBoost()
	{
		_initialFactorBonusValue = _timeBetweenAttacks.FactorBonusValue;

		_isBoostActive = true;
		_timeBetweenAttacks.FactorBonusValue *= SpeedMultiplier.FinalValue;
		_timeBetweenAttacks.ForceRefreshVisual();

		OnGainAttackSpeed?.Invoke();
		
		yield return new WaitForSeconds(EffectLength);

		ResetValue();
		OnLoseAttackSpeed?.Invoke();
		_timeBetweenAttacks.ForceRefreshVisual();
	}

	private void ResetValue()
	{
		_isBoostActive = false;
		float potentialValue = _timeBetweenAttacks.FactorBonusValue / SpeedMultiplier.FinalValue;
		if (potentialValue == _initialFactorBonusValue)
		{
			// No additional boost applied during effect
			_timeBetweenAttacks.FactorBonusValue = _initialFactorBonusValue;
		}
		else
		{
			float boostApplied = potentialValue - _initialFactorBonusValue;

			_timeBetweenAttacks.FactorBonusValue = _initialFactorBonusValue * boostApplied;

			// Log or handle the fact that a boost was applied during the countdown
			Debug.Log($"Boost of {boostApplied} applied during the AttackSpeedOnKill countdown.");
		}
	}

	private void ResetBoostTimer()
	{
		StopAllCoroutines();
		ResetValue();
		StartCoroutine(ApplyAttackSpeedBoost());
	}

	private void OnDestroy()
	{
		foreach (var turret in _turrets)
			turret.OnProjectileKill -= AttackSpeedBoost;
	}
}
