using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class EconomyManager : MonoStatic<EconomyManager>
{
	private int _gold = 10;
	private int _income = 0;
	private int _incomeBonus = 0;
	private float _incomeMultiplier = 1;
	public int Gold { get => _gold; }
	public int Income { get => _income; set => _income = value; }
	public int IncomeBonus { get => _incomeBonus; set => _incomeBonus = value; }
	public float IncomeMultiplier { get => _incomeMultiplier; set => _incomeMultiplier = value; }
	private bool _giveNoGoldThisWave = false;

	public int TotalIncomeThisWave()
	{
		if (_giveNoGoldThisWave)
		{
			return 0;
		}
		return (int)Mathf.Ceil((_income + _incomeBonus) * _incomeMultiplier);	
	}

	public int IncomeToGive()
	{
		if (_giveNoGoldThisWave)
		{
			_giveNoGoldThisWave = false;
			return 0;
		}
		NewWave();
		return (int)Mathf.Ceil((_income + _incomeBonus) * _incomeMultiplier);
	}

	public UnityEvent OnGoldAdded = new();

	public void NewWave()
	{
		StartCoroutine(WaitBeforeNewWave(3));
	}

	private IEnumerator WaitBeforeNewWave(float seconds)
	{
		yield return new WaitForSecondsRealtime(seconds);
		_incomeBonus = 0;
		_incomeMultiplier = 1;
		_giveNoGoldThisWave = false;
	}

	public void GiveNoGoldNextWave()
	{
		_giveNoGoldThisWave = true;
	}
	public void AddBonusGold(int amount)
	{
		_incomeBonus += amount;
	}
	public void AddBonusGoldNextWave(int amount)
	{
		StartCoroutine(NextWaveBonus(amount));
	}
	private IEnumerator NextWaveBonus(int amount)
	{
		int currentWave = GameSystem.Instance.WaveCounter;
		yield return new WaitUntil(() => GameSystem.Instance.WaveCounter > currentWave);
		AddBonusGold(amount);
	}
	public void MultiplyIncomeNextWave(float multiplier)
	{
		StartCoroutine(NextWaveMultiplier(multiplier));
	}
	private IEnumerator NextWaveMultiplier(float multiplier)
	{
		int currentWave = GameSystem.Instance.WaveCounter;
		yield return new WaitUntil(() => GameSystem.Instance.WaveCounter > currentWave);
		IncomeMultiplier += multiplier;
	}

	public void AddGold(int amount = 1)
	{
		_gold += amount;
		OnGoldAdded?.Invoke();
	}
	public bool RemoveGold(int amount)
	{
		if (_gold < amount) return false;
		_gold -= amount;
		return true;
	}
}
