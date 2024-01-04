using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerStructure : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private static readonly float s_showHoverMenuDelay = 0.2f;

	[SerializeField] private string _name;
	public string Name { get => string.IsNullOrEmpty(_name) ? "[ERROR] NO NAME ASSIGNED" : _name; }
	[SerializeField] private List<Stat> _stats;
	[SerializeField] private GameObject _model;
	[SerializeField] private GameObject _statsMenu;
	[SerializeField] private BoxCollider[] _placementColliders;
	[SerializeField] private int _cost;
	[SerializeField] private int _costIncrease;
	[SerializeField] private int _upgradeCostTier2;
	[SerializeField] private int _upgradeCostTier3;
	//1 is light, 2 is normal, 3 is heavy => used for visual feedback when placing
	[SerializeField] PlacementWeight _visualWeight;
	public List<Stat> Stats { get => _stats; set => _stats = value; }
	public GameObject Model { get => _model; }
	public Collider[] PlacementColliders { get => _placementColliders; }
	public PlacementWeight VisualWeight { get => _visualWeight;  }
	public int Cost { get => _cost; set => _cost = value; }
	public int UpgradeCostTier2 { get => _upgradeCostTier2; }
	public int UpgradeCostTier3 { get => _upgradeCostTier3; }
    public int CostIncrease { get => _costIncrease;  }

	private Blueprint _originBlueprint;
    public Blueprint OriginBlueprint 
	{
		get => _originBlueprint;
		set
		{
			_originBlueprint = value;
			if (_variantCanvas != null && OriginBlueprint != null)
			{
				bool isVariant = OriginBlueprint.Variant != null;
				_variantCanvas.SetActive(isVariant);
				if (isVariant)
				{
					_variantCanvas.GetComponentInChildren<Image>().sprite = OriginBlueprint.Variant.WorldIcon;
				}
			}
		}
	}

    private List<Turret> _turrets = new();
	private bool _placedThisWave = true;
	public UnityEvent OnAwake;
	public UnityEvent OnStartDestroy;
	[SerializeField] private bool _hasDestroyAnimation = true;
	[SerializeField] private GameObject _variantCanvasPrefab;
	private GameObject _variantCanvas;

	private void Awake()
	{
		GameSystem.OnRewardClaimed += WavePassed;
		_turrets.AddRange(GetComponents<Turret>());
		HideStatsMenu();
		if (_variantCanvasPrefab != null) 
		{ 
			_variantCanvas = Instantiate(_variantCanvasPrefab,transform.position,Quaternion.identity);
			_variantCanvas.transform.SetParent(transform,true);
			_variantCanvas.transform.localPosition = new(0, 1, 0);
		}
		OnAwake.Invoke();
	}

	private void Start()
	{
		HideHoverMenu();
	}

	private void WavePassed()
	{
		_placedThisWave = false;
		GameSystem.OnRewardClaimed -= WavePassed;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		BuildHud.HoveredStructure = this;
		StartCoroutine(ShowHoverMenuCoroutine());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if(BuildHud.HoveredStructure==this)
			BuildHud.HoveredStructure = null;
		StopAllCoroutines();
		HideHoverMenu();
		HideStatsMenu();
	}

	#region StatsMenu
	public void ShowStatsMenu()
	{
		if (_statsMenu && Hotbar.CurrentActivePlacer == null)
		{
            _statsMenu.SetActive(true);
		}
	}
	public void HideStatsMenu()
	{
		if (_statsMenu)
		{
			_statsMenu.SetActive(false);
		}
	}
	#endregion
	#region Hovermenu
	private IEnumerator ShowHoverMenuCoroutine()
	{
		yield return new WaitForSeconds(s_showHoverMenuDelay);
		ShowHoverMenu();
	}

	private void ShowHoverMenu()
	{
		if (Hotbar.CurrentActivePlacer != null)
			return;
		if (_turrets.Count <= 0)
		{
			Machine.SetConnectionVisibility(true);
			return;
		}
		foreach (Turret turret in _turrets)
		{
			if (turret != null)
				turret.SetTargetBoxesVisual(true);
		}
	}
	public void StartDestroy()
	{
		OnStartDestroy.Invoke();
		StartCoroutine(DestroyAfterTime());
        this.enabled = false;
        return;
	}
	private IEnumerator DestroyAfterTime()
	{	if(_hasDestroyAnimation)
			yield return new WaitForSeconds(1);
		Destroy(this.gameObject);
		yield break;
	}
	private void HideHoverMenu()
	{
		if (_turrets.Count <= 0)
		{
			Machine.SetConnectionVisibility(false);
			return;
		}
		foreach (Turret turret in _turrets)
		{
			if (turret != null)
				turret.SetTargetBoxesVisual(false);
		}
	}

	private void OnDestroy()
	{
		if(OriginBlueprint!=null)
			OriginBlueprint.Structure.Cost -= _costIncrease;
        if (_placedThisWave)
		{
			if (EconomyManager.Instance&& OriginBlueprint != null)
				EconomyManager.Instance.AddGold(OriginBlueprint.Structure.Cost);
		}
		GameSystem.OnRewardClaimed -= WavePassed;
		
	}
	#endregion
}
