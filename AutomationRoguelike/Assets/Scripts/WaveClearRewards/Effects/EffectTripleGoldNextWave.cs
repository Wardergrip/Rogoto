using UnityEngine;

public class EffectTripleGoldNextWave : Effect
{
	[SerializeField] private float _multiplier = 2;
	public override void Excecute()
	{
		EconomyManager.Instance.GiveNoGoldNextWave();
		EconomyManager.Instance.MultiplyIncomeNextWave(_multiplier);
	}
}
