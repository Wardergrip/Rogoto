using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToxicDamageOverTime : DamageOverTime
{
	public ToxicDamageOverTime(Health health, int damagePerTick, float timeLeft) : base("ToxicDamageOverTime", health, damagePerTick, 0, timeLeft,false)
	{
		_damageType = Health.DamageType.Toxic;
	}

	public override void Reapply(Hediff hediff)
	{
		ToxicDamageOverTime toxicDot = (ToxicDamageOverTime)hediff;
		_damagePerTick = Mathf.Max(toxicDot._damagePerTick, _damagePerTick);
		TimeLeft = toxicDot.TimeLeft;
	}
}

public class ToxicWasteResidue : MonoExplosion
{
	public static float s_TickTime { get => 0.4f; }

    [SerializeField] private BoxCollider _collider;
	[Header("Effect vars")]
	[SerializeField] private float _effectTime = 1;
	[SerializeField] private float _slowPercentage = 0.25f;
	private readonly List<HediffHandler> _hediffHandlers = new();

	private void Awake()
	{
		Debug.Assert(_collider.isTrigger);
		transform.position = SnapToGrid(transform.position);
		transform.position += transform.forward; // Without this the snap to grid is off by one tile in the forward direction
		StartCoroutine(TickCoroutine());
	}

	private IEnumerator TickCoroutine()
	{
		while (true)
		{
			yield return new WaitUntil(() => _hediffHandlers.Count > 0);
			foreach (HediffHandler handler in _hediffHandlers) 
			{
				if (handler) ApplyEffect(handler);
			}
			yield return new WaitForSeconds(s_TickTime);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!other.TryGetComponent(out HediffHandler hh)) return;
		_hediffHandlers.Add(hh);
	}

	private void OnTriggerExit(Collider other)
	{
		if (!other.TryGetComponent(out HediffHandler hh)) return;
		_hediffHandlers.Remove(hh);
	}

	private void ApplyEffect(HediffHandler hh)
	{
		hh.AddHediff(new SlowHediff("ToxicWasteSlow", hh, _effectTime, _slowPercentage));
		hh.AddHediff(new ToxicDamageOverTime(hh.Health, Damage, _effectTime));
		if (hh.Health.HealthAmount > 0)
			Projectile.OnHit.Invoke(new ProjectileHitData(hh, Projectile));
		else
			Projectile.OnKill.Invoke(new ProjectileHitData(hh, Projectile));
	}

	private Vector3 SnapToGrid(Vector3 position)
	{
		int layerClamp = 0;
		Vector3 snapped = new(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
		Vector3 clamped = new(snapped.x, layerClamp, snapped.z);
		return clamped;
	}
}