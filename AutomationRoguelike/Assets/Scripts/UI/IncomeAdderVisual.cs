using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IncomeAdderVisual : MonoBehaviour
{
	private Transform _goldCounter;
	[SerializeField] private MoveToUIElement _goldAddUIPreFab;
	[SerializeField] private Transform _spawnGoldPoint;
	[SerializeField] private float totalTime = 2f;
	private int _incomeToGive;
	private List<int> _incomeList = new();

	public UnityEvent OnIncomeSpawned;
	

	void Start()
	{
		_goldCounter = transform.parent;
		_incomeToGive = EconomyManager.Instance.IncomeToGive();
		StartCoroutine(AddIncomeVisual());
	}

	IEnumerator AddIncomeVisual()
	{
		//float timeBetween = _timeBetweenGoldVisualSpawns;
		int chunkSize = 1;

		int numOfChunks = 0;
		int income = _incomeToGive;

		while (income > 0)
		{
			if (income > 10000000)
				chunkSize = 1000000;
			else if (income > 1000000)
				chunkSize = 100000;
			else if (income > 100000)
				chunkSize = 10000;
			else if (income > 10000)
				chunkSize = 1000;
			else if (income > 1000)
				chunkSize = 100;
			else if (income > 100)
				chunkSize = 10;
			else
				chunkSize = 1;

			income -= chunkSize;
			numOfChunks++;
		}

		float timeBetween = 2f/numOfChunks;
		int incomeGiven = 0;
		int chunksGiven = 0;

		while (_incomeToGive > 0)
		{
			if (_incomeToGive > 10000000)
				chunkSize = 1000000;
			else if(_incomeToGive > 1000000)
				chunkSize = 100000;
			else if(_incomeToGive > 100000)
				chunkSize = 10000;
			else if(_incomeToGive > 10000)
				chunkSize = 1000;
			else if (_incomeToGive > 1000)
				chunkSize = 100;
			else if (_incomeToGive > 100)
				chunkSize = 10;
			else
				chunkSize = 1;

			_goldAddUIPreFab.gameObject.SetActive(false);
			MoveToUIElement element = Instantiate(_goldAddUIPreFab, _spawnGoldPoint.position, Quaternion.identity, transform.parent);
			element.Target = _goldCounter;
			element.OnReachedTarget += GoldUIElementReachedGoldCounter;
			element.gameObject.SetActive(true);
			OnIncomeSpawned?.Invoke();
			incomeGiven += chunkSize;
			chunksGiven++;

			_incomeList.Add(chunkSize);
			_incomeToGive -= chunkSize;
			yield return new WaitForSeconds(timeBetween);
		}
		Destroy(this.gameObject);
	}
	private void GoldUIElementReachedGoldCounter()
	{
		EconomyManager.Instance.AddGold(_incomeList[0]);
		_incomeList.RemoveAt(0);
	}
}
