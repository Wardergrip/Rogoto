using UnityEngine;

public class OverdriveVariant : MonoVariant
{
	[SerializeField] private float _attackSpeedDuration = 1;
	[SerializeField] private OverdriveSpeedMultiplier _attackSpeedMultiplier;
	public override bool IsTurret => true;

	public override void Setup(PlayerStructure playerStructure)
	{
		if (playerStructure.TryGetComponent(out Turret turret))
		{
			AttackSpeedOnKill attackSpeedOnKill = turret.gameObject.AddComponent<AttackSpeedOnKill>();
			OverdriveSpeedMultiplier overdriveSpeedMultiplier = turret.gameObject.AddComponent<OverdriveSpeedMultiplier>();
			overdriveSpeedMultiplier.CopyValuesOf(_attackSpeedMultiplier);
			turret.PlayerStructure.Stats.Add(overdriveSpeedMultiplier);
			attackSpeedOnKill.SpeedMultiplier = overdriveSpeedMultiplier;
			attackSpeedOnKill.EffectLength = _attackSpeedDuration;
			return;
		}
		Debug.LogWarning($"OverdriveVariant applied on something without turret script");
	}
}
