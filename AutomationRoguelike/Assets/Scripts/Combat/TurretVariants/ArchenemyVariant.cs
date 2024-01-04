using UnityEngine;

public class ArchenemyVariant : MonoVariant
{
	[SerializeField] private ArchEnemyPercentDamageIncreasePerHit _damageIncreasePerHit;
	public override bool IsTurret => true;

	public override void Setup(PlayerStructure playerStructure)
	{
		if (playerStructure.TryGetComponent(out Turret turret))
		{
			DamageOnHit damageOnHit = turret.gameObject.AddComponent<DamageOnHit>();
			ArchEnemyPercentDamageIncreasePerHit archEnemyPercentDamageIncreasePerHit = turret.gameObject.AddComponent<ArchEnemyPercentDamageIncreasePerHit>();
			turret.PlayerStructure.Stats.Add(archEnemyPercentDamageIncreasePerHit);
			archEnemyPercentDamageIncreasePerHit.CopyValuesOf(_damageIncreasePerHit);
			damageOnHit.PercentDamageIncreasePerHit = archEnemyPercentDamageIncreasePerHit;
			return;
		}
		Debug.LogWarning($"ArchEnemyVariant applied on something without turret script");
	}
}
