using UnityEngine;

public class VoltageVariant : MonoVariant
{
	[SerializeField] private VoltageDamageStat _damagePercent;
	[SerializeField] private float _chanceToProc = .5f;
	[SerializeField] private float _chanceToJump = .5f;
	[SerializeField] private float _range = 10f;
	[SerializeField] private GameObject _visual;
	[SerializeField] private VoltageDamageStat _voltageDamage;
	public override bool IsTurret => true;

	public override void Setup(PlayerStructure playerStructure)
	{
		if (playerStructure.TryGetComponent(out Turret turret))
		{
			Voltage volt = turret.gameObject.AddComponent<Voltage>();
			volt.DamagePercent = _damagePercent;
			volt.ChanceToProc = _chanceToProc;
			volt.ChanceToJump = _chanceToJump;
			volt.Range = _range;
			volt.Visual = _visual;
            turret.PlayerStructure.Stats.Add(_damagePercent);
            volt.UpdateDamage();
			return;
		}
		Debug.LogWarning($"VoltageVariant applied on something without turret script");
	}
}
