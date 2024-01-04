using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class DamageExplosion : MonoExplosion
{
	private bool _canDamage = true;

	private void Start()
	{
		Debug.Assert(GetComponent<SphereCollider>().isTrigger);
		StartCoroutine(SetCanDamageFalse());
	}

	private IEnumerator SetCanDamageFalse()
	{
		yield return null;
		_canDamage = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!_canDamage) return;
		if (!other.TryGetComponent(out HediffHandler hediffHandler)) return;
		hediffHandler.Health.Damage(Damage);
		if (hediffHandler.Health.HealthAmount > 0)
		{
			Projectile.OnHit.Invoke(new ProjectileHitData(hediffHandler, Projectile));
		}
		else
		{
			Projectile.OnKill.Invoke(new ProjectileHitData(hediffHandler, Projectile));
		}
	}
}
