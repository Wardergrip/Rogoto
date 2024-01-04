using System.Collections.Generic;
using System.Linq;
using Trivial;
using UnityEngine;

public class BleedOnHit : MonoBehaviour
{
	private List<Turret> _turrets = new();
	public RazorsharpBleedDamage DamagePerTick { get; set; }
	public float EffectLength { get; set; } = 1;
	public float BleedChance { get; set; } = 0.5f;

	private void Awake()
	{
		_turrets = GetComponents<Turret>().ToList();
		foreach (var turret in _turrets)
			turret.OnProjectileHit += BleedHit;
	}

	private void BleedHit(ProjectileHitData projectileHitData)
	{
		if (!RandomUtils.YesOrNo(BleedChance, Mathf.Clamp01(1 - BleedChance)))
        {
			return;
        }
        Health health = projectileHitData.HediffHandler.Health;
		projectileHitData.HediffHandler.AddHediff(new Bleed(health, (int)DamagePerTick.FinalValue,0, EffectLength));
	}

	private void OnDestroy()
	{
		foreach (var turret in _turrets)
			turret.OnProjectileHit -= BleedHit;
	}
}
