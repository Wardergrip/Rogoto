using UnityEngine;

public class EffectSavedGoldBonus : Effect
{
	[SerializeField] private float _percentage = 0.2f;

	public override void Excecute()
	{
		EconomyManager.Instance.Income += ((int)Mathf.Round(EconomyManager.Instance.Gold * _percentage));
	}
	public override bool IsAvailable()
	{
		return (int)Mathf.Round(EconomyManager.Instance.Gold * _percentage) >= 1;
	}
}
