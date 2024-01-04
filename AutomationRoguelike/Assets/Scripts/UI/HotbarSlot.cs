using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HotbarSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Blueprint _blueprint;
    [SerializeField] private Image _visual;
    [SerializeField] private float _hoverTime = 0.75f;
    [SerializeField] private HotbarSlotInfoPanel _hotbarSlotInfoPanel;

	public Image Visual { get { return _visual; } }

    public Blueprint Blueprint { get => _blueprint;  }

    private void Awake()
    {
        if (_visual) _visual.gameObject.SetActive(false);
        _hotbarSlotInfoPanel.Slot = this;
        
    }
    public void SetNewBlueprint(Blueprint blueprint)
    {
        _blueprint = blueprint;
        _visual.sprite = blueprint.Icon;
        _visual.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartCoroutine(HoverCoroutine());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StopAllCoroutines();
        HideInfo();
    }
    private IEnumerator HoverCoroutine()
    {
        yield return new WaitForSeconds(_hoverTime);
        ShowInfo();
    }

    private void ShowInfo()
    {
        _hotbarSlotInfoPanel.Show();
    }
    private void HideInfo()
    {
        _hotbarSlotInfoPanel.Hide();
    }
}
