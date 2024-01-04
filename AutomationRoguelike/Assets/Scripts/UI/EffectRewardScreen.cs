using TMPro;
using UnityEngine;

public class EffectRewardScreen : MonoBehaviour
{
	private ResourceCounters _goldCounter;
	[SerializeField] GameObject _goldAdderObject;
	[SerializeField] GameObject _effectCardPreFab;
	[SerializeField] EffectUICard _effectCard1;
	[SerializeField] EffectUICard _effectCard2;
	[SerializeField] EffectUICard _effectCard3;

	[SerializeField] TextMeshProUGUI _incomeThisWaveText;
	[SerializeField] TextMeshProUGUI _bonusIncomeThisWaveText;
	
	[SerializeField] private Animator _backGroundAnimator;
	public bool RewardIsTaken { get; private set; } = false;

	private void Awake()
	{
		GameSystem.OnEnemyAllKilled += GiveReward;
	}
	private void Start()
	{
		_goldCounter = FindObjectOfType<ResourceCounters>();     
	}
	private void OnEnable()
	{
		_backGroundAnimator.SetTrigger("Enter");
	}
	public void SkipReward()
	{
		RewardChosen();
	}
	public void DisableCards()
	{
		_effectCard1.gameObject.SetActive(false);
		_effectCard2.gameObject.SetActive(false);
		_effectCard3.gameObject.SetActive(false);
	}
	private void ResetCards()
	{
		_effectCard1.TurnReset();
		_effectCard2.TurnReset();
		_effectCard3.TurnReset();
	}
	public void RewardChosen()
	{
		RewardIsTaken = true;
		ResetCards();
		_incomeThisWaveText.text = "Income this wave: " + EconomyManager.Instance.TotalIncomeThisWave().ToString();
		_bonusIncomeThisWaveText.text = "Bonus Income this wave:0 " ;
		Instantiate(_goldAdderObject, _goldCounter.transform);
		GameSystem.Instance.RewardClaimed();
	}
	private void GiveReward()
	{
		RewardIsTaken = false;
		_incomeThisWaveText.text = "Income this wave: " + EconomyManager.Instance.Income.ToString();
		_bonusIncomeThisWaveText.text = "Bonus Income this wave: " + EconomyManager.Instance.IncomeBonus.ToString();
			_effectCard1.gameObject.SetActive(true);
			_effectCard1.SetNewEffect();
			_effectCard2.gameObject.SetActive(true);
			_effectCard2.SetNewEffect();
			_effectCard3.gameObject.SetActive(true);
			_effectCard3.SetNewEffect();
	}
	private void OnDestroy()
	{
		GameSystem.OnEnemyAllKilled -= GiveReward;
	}
}
