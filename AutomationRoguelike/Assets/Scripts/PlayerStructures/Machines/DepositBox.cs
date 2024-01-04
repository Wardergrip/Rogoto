
using TMPro;
using UnityEngine;

public class DepositBox : Machine
{
	[SerializeField] TextMeshProUGUI _totalIncomeCounter;
	private int _totalIncomeFromBox;

	public int TotalIncomeFromBox { get => _totalIncomeFromBox;  }

	protected override void ProcessResource()
	{
		EconomyManager inst = EconomyManager.Instance;
		if (inst != null) inst.Income -= _totalIncomeFromBox;
		int totalIncomeFromBox = 0;
		foreach (InputConnection input in _inputs)
		{
			if (input.ConnectedOutput != null)
			{
				totalIncomeFromBox += input.ConnectedOutput.Value;
			}
		}

		_totalIncomeCounter.text = "+"+ totalIncomeFromBox.ToString();
		_totalIncomeFromBox = totalIncomeFromBox;
		if (inst != null) inst.Income += _totalIncomeFromBox;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		EconomyManager inst = EconomyManager.Instance;
		if (inst == null) return;
		inst.Income -= _totalIncomeFromBox;
	}
}
