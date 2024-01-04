using UnityEngine;

public class EffectDecreaseBaseHpIncreaseIncome : Effect
{
	[SerializeField] private float _baseHpDecrease = 0.1f;
	[SerializeField] private float _additonalIncomePercentage = 0.1f;

	public override void Excecute()
	{
		Base.Instance.Health.DecreaseMaxHealth((int)(_baseHpDecrease  * Base.Instance.Health.MaxHealth));
		EconomyManager.Instance.IncomeBonus = Mathf.CeilToInt(EconomyManager.Instance.IncomeBonus * (1 + _additonalIncomePercentage));
		BaseHealthBarIdentifier.Instance.HealthBar.SeperationsInHealthBar -= 1;
	}
}
