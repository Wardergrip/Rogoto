using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HotbarSlotInfoPanel : MonoBehaviour
{
	[SerializeField] private Image _panel;
	[SerializeField] private TextMeshProUGUI _nameText;
	[SerializeField] private TextMeshProUGUI _descriptionText;
	[SerializeField] private TextMeshProUGUI _statsText;
	[SerializeField] private TextMeshProUGUI _costText;
	[SerializeField] private Transform _statParent;
	[SerializeField] private StatsHover _statHover;
	[SerializeField] private Image _variantChip;
	public HotbarSlot Slot { get; set; }

	private void Awake()
	{
		Hide();
	}

	public void Show()
	{
		if (Slot.Blueprint == null)
			return;

		PlayerStructure structure = Slot.Blueprint.Structure;

		_nameText.text = Slot.Blueprint.Name;
		_descriptionText.text = Slot.Blueprint.DescriptionText;
		_costText.text = $"<sprite name=Cost> {structure.Cost}";
		bool isVariant = Slot.Blueprint.Variant != null;
		_variantChip.sprite = isVariant ? Slot.Blueprint.Variant.Sprite : null;
		if (!isVariant)
		{
			_variantChip.gameObject.SetActive(false);
		}

		foreach (Transform child in _statParent)
		{
			Destroy(child.gameObject);
		}

		for (int i = 0; i < Slot.Blueprint.Structure.Stats.Count; ++i)
		{
			StatsHover newStat = Instantiate(_statHover, _statParent);
			newStat.InitializeStat(structure.Stats[i], structure.Stats.Count);
		}

		SetVisibility(true);
	}

	public void Hide()
	{
		SetVisibility(false);
	}

	private void SetVisibility(bool visible)
	{
		bool isVariant = false;
		if (Slot != null && Slot.Blueprint != null)
		{
			isVariant = Slot.Blueprint.Variant != null;
		}

		_variantChip.gameObject.SetActive(visible && isVariant);
		_panel.gameObject.SetActive(visible);
		_nameText.gameObject.SetActive(visible);
		_descriptionText.gameObject.SetActive(visible);
		_statsText.gameObject.SetActive(visible);
		_costText.gameObject.SetActive(visible);
		_statParent.gameObject.SetActive(visible);
	}
}
