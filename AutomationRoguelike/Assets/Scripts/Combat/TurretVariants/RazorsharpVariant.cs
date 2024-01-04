using UnityEngine;

public class RazorsharpVariant : MonoVariant
{
	[SerializeField] private RazorsharpBleedDamage _damagePerTick;
	[SerializeField] private float _bleedDuration = 1;
	public override bool IsTurret => true;

	public override void Setup(PlayerStructure playerStructure)
	{
		if (playerStructure.TryGetComponent(out Turret turret))
		{
			BleedOnHit bleedOnHit = turret.gameObject.AddComponent<BleedOnHit>();
			RazorsharpBleedDamage razorsharpBleedDamage = turret.gameObject.AddComponent<RazorsharpBleedDamage>();
			razorsharpBleedDamage.CopyValuesOf(_damagePerTick);
			turret.PlayerStructure.Stats.Add(razorsharpBleedDamage);
			bleedOnHit.DamagePerTick = razorsharpBleedDamage;
			bleedOnHit.EffectLength = _bleedDuration;
			return;
		}
		Debug.LogWarning($"RazorsharpVariant applied on something without turret script");
	}
}
