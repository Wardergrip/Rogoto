using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DamageOnHit : MonoBehaviour
{
	private List<Turret> _turrets = new();
	private Damage _damage;
	[Tooltip("Percent of max damage that is added each hit")]
	public ArchEnemyPercentDamageIncreasePerHit PercentDamageIncreasePerHit { get; set; }
	private float _damageIncreasePerHit = 1;
	private float _damageIncrease = 0;

	private void Awake()
	{
		_turrets = GetComponents<Turret>().ToList();
		_turrets[0].gameObject.TryGetComponent(out Damage damage);		
		_damage = damage;
		foreach (var turret in _turrets)
		{
			turret.OnProjectileHit += IncreaseDamage;
			turret.OnSwapTarget += ResetDamage;
		}
		GameSystem.OnEnemyStartSpawning += CheckDamage;
	}

	private void IncreaseDamage(ProjectileHitData projectileHitData)
	{
		_damageIncrease += _damageIncreasePerHit;
		_damage.FlatBonusValue += _damageIncreasePerHit;
	}

	private void ResetDamage(ProjectileHitData projectileHitData)
	{
		ResetDamage();
	}
	private void ResetDamage(Turret turret)
	{
		ResetDamage();
	}
	private void ResetDamage()
	{
		_damage.FlatBonusValue -= _damageIncrease;
		_damageIncrease = 0;
	}
	private void CheckDamage()
	{
		_damageIncreasePerHit = Mathf.Ceil(_damage.BaseValue * PercentDamageIncreasePerHit.FinalValue);
	}

	private void OnDestroy()
	{
		foreach (var turret in _turrets)
		{
			turret.OnProjectileHit -= IncreaseDamage;
			turret.OnProjectileKill -= ResetDamage;
			turret.OnSwapTarget -= ResetDamage;
		}
		GameSystem.OnEnemyStartSpawning -= CheckDamage;
	}
}
