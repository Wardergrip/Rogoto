using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceCounters : MonoBehaviour
{
	[SerializeField] private string _goldCounterPrefix = "Gold: ";
	[SerializeField] private TextMeshProUGUI _goldCounter;
	[SerializeField] private string _goldCounterPostfix = "";    
	[SerializeField] private string _incomeCounterPrefix = "Income: ";
	[SerializeField] private TextMeshProUGUI _incomeCounter;
	[SerializeField] private string _incomeCounterPostfix = "";

	private void Update()
	{
		_goldCounter.text = $"{_goldCounterPrefix}{EconomyManager.Instance.Gold}{_goldCounterPostfix}" ;
		_incomeCounter.text = $"{_incomeCounterPrefix}{EconomyManager.Instance.Income}{_incomeCounterPostfix}" ;
	}
}
