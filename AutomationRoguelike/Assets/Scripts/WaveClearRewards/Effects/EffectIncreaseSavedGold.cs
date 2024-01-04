using UnityEngine;

public class EffectIncreaseSavedGold : Effect
{
	[SerializeField] private float _multiplier = 1.5f;
	public override void Excecute()
	{
		int gold = EconomyManager.Instance.Gold;
		EconomyManager.Instance.AddGold((int)Mathf.Ceil((gold * _multiplier) - gold));
	}
}
