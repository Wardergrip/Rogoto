using System.Collections.Generic;
using System.Linq;
using Trivial;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class Voltage : MonoBehaviour
{
	private List<Turret> _turrets = new();
	public VoltageDamageStat DamagePercent { get; set; }
	public float ChanceToProc { get; set; } = 0.5f;
	public float ChanceToJump { get; set; } = 0.5f;
	public float Range { get; set; } = 10f;
	public GameObject Visual { get; set; } = null;
	private Enemy _enemy = new();
	private List<Enemy> _enemiesHit = new();
	private int _damage;

	private void Awake()
	{
		_turrets = GetComponents<Turret>().ToList();
		foreach (var turret in _turrets)
		{
			turret.OnProjectileHit += TriggerVoltage;
		}
		
		_turrets[0].OnTierChanged.AddListener(UpdateDamage);
	}

	public void UpdateDamage()
	{
		_damage = (int)Mathf.Ceil(_turrets[0].Damage.BaseValue * DamagePercent.FinalValue);
	}

	private void TriggerVoltage(ProjectileHitData projectileHitData)
	{
		UpdateDamage();
		if (!RandomUtils.YesOrNo(ChanceToProc, Mathf.Clamp01(1 - ChanceToProc)))
		{
			return;
		}
		_enemiesHit.Clear();
		HediffHandler heddif = projectileHitData.HediffHandler;
		heddif.Health.Damage(_damage, Health.DamageType.Voltage);
		_enemy = heddif.Enemy;
		Instantiate(Visual, _enemy.transform.position, Visual.transform.rotation, transform);
		Jump(_enemy);
	}

	private void Jump(Enemy initialEnemy)
	{
		
		if (!RandomUtils.YesOrNo(ChanceToJump, Mathf.Clamp01(1 - ChanceToJump)))
		{
			return;
		}

		Enemy[] enemiesInRange = FindEnemiesInRange(transform.position, Range);

		enemiesInRange = enemiesInRange.Where(enemy => enemy != initialEnemy && !_enemiesHit.Contains(enemy)).ToArray();

		Enemy nearestEnemy = FindNearestEnemy(transform.position, enemiesInRange);

		if (nearestEnemy != null)
		{
			_enemiesHit.Add(initialEnemy);
			nearestEnemy.Health.Damage(_damage, Health.DamageType.Voltage);
			GameObject vis =Instantiate(Visual, nearestEnemy.transform.position, Visual.transform.rotation, transform);
			vis.GetComponent<VoltageConnection>().Connect(initialEnemy.gameObject.transform.position, nearestEnemy.gameObject.transform.position);

			Jump(nearestEnemy);
		}
	}

	private Enemy[] FindEnemiesInRange(Vector3 position, float range)
	{

		Collider[] colliders = Physics.OverlapSphere(position, range);

		Enemy[] enemies = colliders
			.Where(collider => collider.GetComponent<Enemy>() != null)
			.Select(collider => collider.GetComponent<Enemy>())
			.ToArray();

		return enemies;
	}

	private Enemy FindNearestEnemy(Vector3 position, Enemy[] enemies)
	{
		float nearestDistance = Mathf.Infinity;
		Enemy nearestEnemy = null;

		foreach (var enemy in enemies)
		{
			float distance = Vector3.Distance(position, enemy.transform.position);
			if (distance < nearestDistance)
			{
				nearestDistance = distance;
				nearestEnemy = enemy;
			}
		}

		return nearestEnemy;
	}

	private void OnDestroy()
	{
		foreach (var turret in _turrets)
			turret.OnProjectileHit -= TriggerVoltage;
		_turrets[0].OnTierChanged.RemoveListener(UpdateDamage);
	}
}
