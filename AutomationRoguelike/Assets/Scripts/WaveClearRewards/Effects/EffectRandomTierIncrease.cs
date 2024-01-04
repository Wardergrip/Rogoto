using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Trivial;

public class EffectRandomTierIncrease : Effect
{
	public override void Excecute()
	{
		if (!IsAvailable())
		{
			Debug.LogWarning($"Effect not available.");
			return;
		}

		List<Turret> upgradeableTurrets = Turret.s_Turrets.Where(x => x.Tier < 3).ToList();
		if (upgradeableTurrets.Count > 0)
		{
			upgradeableTurrets.GetRandomValue().Upgrade();
			return;
		}
	}
	public override bool IsAvailable()
	{
		return Turret.s_Turrets.Any(x => x.Tier < 3);
	}
}
