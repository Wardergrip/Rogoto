using UnityEngine;

public class EffectPercentIncomeThisWave : Effect
{
	[SerializeField] private float _percentage = 0.5f;
	public override void Excecute()
	{
		EconomyManager.Instance.IncomeBonus += (int)Mathf.Round((EconomyManager.Instance.Income * _percentage));
	}
}
