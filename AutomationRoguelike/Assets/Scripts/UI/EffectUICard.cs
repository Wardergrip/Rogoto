using System.Collections.Generic;
using System.Linq;
using TMPro;
using Trivial;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum EffectRarity
{
	Common, Rare, Legendary
}
[System.Serializable]
public struct StatIcon
{
	public Sprite Icon;
	public EffectStatIcon Stat;
}
public class EffectUICard : MonoBehaviour
{
	[SerializeField] private Effect _buffTurretEffect;
	[SerializeField] private float _buuffTurretChanceToGet = 3f;
	[SerializeField] private LookAtMousePos _lookAtMouse;
	[SerializeField] private Effect[] _effectsCommon;
	[SerializeField] private Effect[] _effectsRare;
	[SerializeField] private Effect[] _effectsLegendary;
	[SerializeField] float _commonChance;
	[SerializeField] float _rareChance;
	[SerializeField] float _legendaryChance;
	[SerializeField] private TextMeshProUGUI _descriptionText;
	[SerializeField] private TextMeshProUGUI _effectNameText;
	[SerializeField] private Image _iconImage;
	[SerializeField] private Image _statIconImagePreFab;
	[SerializeField] private GameObject _statIconParent;
	[SerializeField] private Image _rimRareBack;
	[SerializeField] private Image _rimRareFront;
	[SerializeField] private Image _rimLegendaryBack;
	[SerializeField] private Image _rimLegendaryFront;
	[SerializeField] private GameObject _frontCardCommon;
	[SerializeField] private GameObject _frontCardRare;
	[SerializeField] private GameObject _frontCardLegendary;
	[SerializeField] private List<StatIcon> _statIcons;
	[SerializeField] private Animator _animatorFront;
	[SerializeField] private Animator _animatorBack;
	[SerializeField] private GameObject _sparkles;
	[SerializeField] private GameObject[] _roulettes;
	[SerializeField] private Image[] _rouletteBackgrounds;
	[SerializeField] private Color _legendaryColor;
	[SerializeField] private Color _rareColor;
	[SerializeField] private Color _commonColor;
    private EffectRarity _pickedRarity;
	private bool _isTurned = false;
	private Effect _pickedEffect;
	private bool _buffEffectPicked = false;
	public UnityEvent OnResetVisuals;
	public UnityEvent OnTurnCard;

	private static List<EffectUICard> _shownCards = new();

	public Effect PickedEffect { get => _pickedEffect; }

	private void Awake()
	{
		_shownCards.Add(this);
		
	}
	public void Hover()
	{
		if (_pickedRarity == EffectRarity.Legendary)
		{
			_rimLegendaryBack.gameObject.SetActive(true); 
		}
		else if (_pickedRarity == EffectRarity.Rare)
		{
			_rimRareBack.gameObject.SetActive(true);
		}
			_lookAtMouse.enabled = true;
	}
	public void StopHover()
	{
		if (_pickedRarity == EffectRarity.Legendary)
		{
			_rimLegendaryBack.gameObject.SetActive(false);
		}
		else if (_pickedRarity == EffectRarity.Rare)
		{
			_rimRareBack.gameObject.SetActive(false);
		}
		_lookAtMouse.enabled = false;
		_lookAtMouse.gameObject.transform.rotation = Quaternion.identity;
	}
	public void RevealFront()
	{
		int amountOfStatIcons = 1;
		if (_pickedRarity == EffectRarity.Legendary)
		{
			CameraController.ShakeCamera(1);
			_frontCardLegendary.SetActive(true);
			_rimLegendaryFront.gameObject.SetActive(true);
			_sparkles.SetActive(true);
			foreach (Image backgr in _rouletteBackgrounds)
			{
				backgr.color = _legendaryColor;
			}
			amountOfStatIcons = 3;
		}
		else if (_pickedRarity == EffectRarity.Rare)
		{
			_frontCardRare.SetActive(true);
			_rimRareFront.gameObject.SetActive(true);
            foreach (Image backgr in _rouletteBackgrounds)
            {
                backgr.color = _rareColor;
            }
            amountOfStatIcons = 2;
		}
		else
		{
            foreach (Image backgr in _rouletteBackgrounds)
            {
                backgr.color = _commonColor;
            }
            _frontCardCommon.SetActive(true);
		}
		for (int i = 0; i < amountOfStatIcons; i++)
		{
			Image statIcon = Instantiate(_statIconImagePreFab, _statIconParent.transform);
			statIcon.sprite = _statIcons.Find(x => x.Stat == _pickedEffect.EffectStatIcon).Icon;
		}
		if (_buffEffectPicked)
		{
			foreach (GameObject gameObject in _roulettes)
			{
				gameObject.SetActive(true);
			}
		}
	}

	private bool CheckIfEffectAlreadyPulled(Effect pick)
	{
		foreach (EffectUICard effectUICard in _shownCards)
		{
			if (effectUICard != this && effectUICard.PickedEffect != null && effectUICard.PickedEffect.Name.Equals(pick.Name))
				return true;
		}
		return false;
	}
	public void SetNewEffect()
	{
		_buffEffectPicked = false;
		if (_buffTurretEffect.IsAvailable() && RandomUtils.YesOrNo(_buuffTurretChanceToGet))
		{
			_pickedRarity=RandomUtils.GetWeightedRandom(
					new WeightedValue<EffectRarity>(_commonChance, EffectRarity.Common),
					new WeightedValue<EffectRarity>(_rareChance + GameSystem.Instance.Luck/2, EffectRarity.Rare),
					new WeightedValue<EffectRarity>(_legendaryChance + GameSystem.Instance.Luck/0.2f, EffectRarity.Legendary));
			_pickedEffect = _buffTurretEffect;
			_buffEffectPicked = true;
		}
		else
		{
			Effect[] rarityArray = GetRarity();
			_pickedEffect = PickRandomEffect(rarityArray);
		}
		_pickedEffect.Init(_pickedRarity);
		_descriptionText.text = _pickedEffect.OverrideDescription();
		_iconImage.sprite = _pickedEffect.Icon;
		_effectNameText.text = _pickedEffect.Name;
	}
	private Effect[] GetRarity()
	{
		Effect[] rarityArray = RandomUtils.GetWeightedRandom(
					new WeightedValue<Effect[]>(_commonChance, _effectsCommon),
					new WeightedValue<Effect[]>(_rareChance + GameSystem.Instance.Luck, _effectsRare),
					new WeightedValue<Effect[]>(_legendaryChance + GameSystem.Instance.Luck, _effectsLegendary));
		if (rarityArray == _effectsCommon)
			_pickedRarity = EffectRarity.Common;
		else if (rarityArray == _effectsRare)
			_pickedRarity = EffectRarity.Rare;
		else
			_pickedRarity = EffectRarity.Legendary;
		return rarityArray;
	}
	private Effect PickRandomEffect(Effect[] array)
	{
		Effect[] pickArray = array.Where(x => x.IsAvailable()).ToArray();
		Effect pick = pickArray[Random.Range(0, pickArray.Length)];
		if (CheckIfEffectAlreadyPulled(pick))
			return PickRandomEffect(pickArray);
		else
			return pick;
  
	}
	public void ChooseThisEffect()
	{
		EffectRewardScreen screen = transform.parent.GetComponent<EffectRewardScreen>();
		if (screen.RewardIsTaken)
			return;
        if (!_isTurned)
		{
			_animatorBack.SetTrigger("Turn");
			_isTurned = true;
			OnTurnCard?.Invoke();
			return;
		}
		_pickedEffect.Excecute();
		screen.RewardChosen();
	}
	public void TurnReset()
	{
		if(_isTurned)
			_animatorFront.SetTrigger("Reset");
		_isTurned = false;
	}
	public void ResetFrontVisuals()
	{
		OnResetVisuals.Invoke();
            foreach (GameObject gameObject in _roulettes)
            {
                gameObject.SetActive(false);
            }
		_sparkles.SetActive(false);
		_frontCardCommon.SetActive(false);
		_frontCardRare.SetActive(false);
		_frontCardLegendary.SetActive(false);
		_rimLegendaryFront.gameObject.SetActive(false);
		_rimLegendaryBack.gameObject.SetActive(false);
		_rimRareBack.gameObject.SetActive(false);
		_rimRareFront.gameObject.SetActive(false);
		
		foreach (Transform child in _statIconParent.transform)
		{
			Destroy(child.gameObject);
		}
	}
	private void OnDestroy()
	{
		_shownCards.Remove(this);
	}
}
