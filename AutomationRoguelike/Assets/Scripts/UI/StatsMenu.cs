using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatsMenu : MonoBehaviour
{
	[SerializeField] private PlayerStructure _playerstructure;
	private Turret _turret;

	[Header("UI elements")]
	[SerializeField] private TextMeshProUGUI _name;
	[SerializeField] private Transform _statParent;
	[SerializeField] private StatsNumbers _statPrefab;
	private readonly List<StatsNumbers> _statList = new();
	[SerializeField] private Button _upgradeButton;
	[SerializeField] private TextMeshProUGUI _upgradeCostText;
	[SerializeField] private TextMeshProUGUI _targetText;
	[SerializeField] private Image _level1;
	[SerializeField] private Image _level2;
	[SerializeField] private Image _level3;
	[SerializeField] private Image _variantChip;

	private int _tier = 1; 

	public void Upgrade()
	{	
		if (!CanAffordNextUpgrade())
		{
			return;
		}

        bool success = EconomyManager.Instance.RemoveGold(GetCurrentUpgradeCost().Value);
        Debug.Assert(success, "Can afford succeeded yet not enough gold");
        _turret.Upgrade();
	}

	public void UpgradeToTier2() 
	{
		_level1.gameObject.SetActive(false);
		_level2.gameObject.SetActive(true);
		_tier = 2;
	}

	public void UpgradeToTier3()
	{
		_level2.gameObject.SetActive(false);
		_level3.gameObject.SetActive(true);
		_tier = 3;
	}

	public void CycleForward()
	{ 
		_targetText.text = _turret.CyclePriorityForward().ToString();
	}

	public void CycleBackward()
	{
		_targetText.text = _turret.CyclePriorityBackward().ToString();
	}


	private void Start()
	{

		for (int i = 0; i < _playerstructure.Stats.Count; ++i)
		{
			StatsNumbers newStat = Instantiate(_statPrefab, _statParent);
			newStat.InitializeStat(_playerstructure.Stats[i], _playerstructure.Stats.Count, i);
			_statList.Add(newStat);
		}
		bool isVariant = _playerstructure.OriginBlueprint.Variant != null;
		_variantChip.sprite = isVariant ? _playerstructure.OriginBlueprint.Variant.Sprite : null;
		if (!isVariant)
		{
			_variantChip.gameObject.SetActive(false);
		}
		_name.text = _playerstructure.Name;
		UpdateStatText();
		if (_playerstructure.gameObject.TryGetComponent(out Turret turret))
		{
			turret.OnTierChanged.AddListener(TurretTierChange);
			turret.OnStatBuffed.AddListener(TurretOnStatBuffed);
			turret.OnStatVisualNeedsRefresh += TurretStatVisualNeedsRefresh;
			_turret = turret;
		}
		EconomyManager.Instance.OnGoldAdded.AddListener(UpdateUpgradeButtonAndCost);
		UpdateUpgradeButtonAndCost();
	}

	private void TurretStatVisualNeedsRefresh(Stat obj)
	{
		UpdateStatText();
		UpdateUpgradeButtonAndCost();
	}

	private void TurretOnStatBuffed(Stat obj)
	{
		UpdateStatText();
		UpdateUpgradeButtonAndCost();
	}

	private void TurretTierChange()
	{
		UpdateStatText();
		UpdateUpgradeButtonAndCost();
	}

	private void UpdateStatText()
	{
		for (int i = 0; i < _playerstructure.Stats.Count; ++i)
		{
			_statList[i].UpdateStat(_playerstructure.Stats[i]);
		}
	}

	private void UpdateUpgradeButtonAndCost()
	{
		int? currentCost = GetCurrentUpgradeCost();
		if (currentCost.HasValue) 
		{
			//_upgradeCostText.text = $"<sprite name=Cost> {currentCost}";
			_upgradeCostText.text = $"{currentCost}";
		}
		else
		{
			_upgradeCostText.text = "MAX";
		}
		_upgradeButton.interactable = (_playerstructure.Stats[0].CurrentTier < 3) && CanAffordNextUpgrade();
	}

	private bool CanAffordNextUpgrade()
	{
		EconomyManager inst = EconomyManager.Instance;
		if (inst == null) return false;
		int? cost = GetCurrentUpgradeCost();
		if (!cost.HasValue) return false;
		return inst.Gold >= cost.Value;
	}
	
	private int? GetCurrentUpgradeCost()
	{
		return _turret.Tier switch
		{
			1 => _playerstructure.UpgradeCostTier2,
			2 => _playerstructure.UpgradeCostTier3,
			3 => null,
			_ => null,
		};
	}

	private void OnDestroy()
	{
		if (_playerstructure.gameObject.TryGetComponent(out Turret turret))
		{
			turret.OnTierChanged.RemoveListener(TurretTierChange);
			turret.OnStatBuffed.RemoveListener(TurretOnStatBuffed);
			turret.OnStatVisualNeedsRefresh -= TurretStatVisualNeedsRefresh;
		}
		EconomyManager inst = EconomyManager.Instance;
		if (inst != null)
		{
			inst.OnGoldAdded.RemoveListener(UpdateUpgradeButtonAndCost);
		}
	}
}
