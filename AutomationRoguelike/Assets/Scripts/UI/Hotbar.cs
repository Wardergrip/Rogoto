using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public enum HotbarType
{
    Machine, Combat
}

public class Hotbar : MonoBehaviour
{
    public static List<Hotbar> s_Hotbars { get; private set; } = new();

    [SerializeField] private HotbarType _hotbarType;
    public HotbarType HotbarType { get => _hotbarType; }
    [SerializeField] private GameObject _selectorObj;
    [SerializeField] private RectTransform _rod;
    [SerializeField] private float _widthPerSlot = 105.0f;
    private float _initialRodWidth;
    [SerializeField] private List<HotbarSlot> _hotbarSlots;
    [SerializeField] private int _minimumSlotsAmount = 3;
    public int CurrentActiveSlotsAmount { get; private set; } = 0;
    public int MaxActiveSlotsAmount { get => _hotbarSlots.Count; }
    [SerializeField] private StructurePlacer _placerPrefab;
    public static StructurePlacer CurrentActivePlacer;
    private int _selectedIdx = -1;

    private bool _isSelectingDisabled = false;
    public bool IsSelectingDisabled
    {
        get { return _isSelectingDisabled; }
        set
        {
            _isSelectingDisabled = value;
            if (value == true) Deselect();
        }
    }
    
    public HotbarSlot SelectedSlot { get { return _selectedIdx == -1 ? null : HotbarSlots[_selectedIdx]; } }

    public List<HotbarSlot> HotbarSlots { get => _hotbarSlots; }

    public UnityEvent OnSelectedIdxChanged;
    [SerializeField] private Blueprint[] _startBluePrints;

    private void Awake()
    {
        _initialRodWidth = _rod.sizeDelta.x;
        s_Hotbars.Add(this);
    }

    private void OnDestroy()
    {
        s_Hotbars.Remove(this);
    }

    private void Start()
    {
        _selectorObj.SetActive(false);
        AddStartBluePrints();
        UpdateHotbar();
    }

    private void UpdateHotbar()
    {
        CurrentActiveSlotsAmount = 0;
        CurrentActiveSlotsAmount = _hotbarSlots.Count(x => x.Blueprint != null);
        CurrentActiveSlotsAmount = Mathf.Max(CurrentActiveSlotsAmount, _minimumSlotsAmount);
        for (int i = 0; i < _hotbarSlots.Count; ++i)
        {
            GameObject slotParentObject = _hotbarSlots[i].transform.parent.gameObject;
            bool state = i < CurrentActiveSlotsAmount;
            slotParentObject.SetActive(state);
            slotParentObject.GetComponent<Button>().interactable = state;
        }
        Vector2 sizeDelta = _rod.sizeDelta;
        sizeDelta.x = _initialRodWidth - (_hotbarSlots.Count * _widthPerSlot) + (CurrentActiveSlotsAmount * _widthPerSlot);
        _rod.sizeDelta = sizeDelta;
    }

    public void AddHotbarSlot()
    {
        ++_minimumSlotsAmount;
        UpdateHotbar();
    }

    private void AddStartBluePrints()
    {
        for (int i = 0; i < _startBluePrints.Length; i++)
        {
            _hotbarSlots[i].SetNewBlueprint(_startBluePrints[i]);
        }
    }
    private void SelectBlueprint()
    {
        CheckDestroyPlacementPreview();

        if (_hotbarSlots[_selectedIdx].Blueprint == null)
            return;

        StructurePlacer placer = Instantiate(_placerPrefab, Vector3.zero, Quaternion.identity);
        placer.SetUp(_hotbarSlots[_selectedIdx].Blueprint);
        CurrentActivePlacer = placer;
    }


    public void Deselect()
    {
        Select(-1);
		foreach (ResourceSpot rs in ResourceSpot.s_ResourceSpots)
		{
			rs.HighlightResource(false);
		}
	}
    private void CheckDestroyPlacementPreview()
    {
        if (CurrentActivePlacer != null)
        {
            CurrentActivePlacer.Remove();
            Destroy(CurrentActivePlacer.gameObject);
        }
        
    }
    public void Select(int idx)
    {
        if (IsSelectingDisabled)
        {
            return;
        }
        if (BuildHud.s_ActiveConveyerLine != null)
        {
            Destroy(BuildHud.s_ActiveConveyerLine.gameObject);
        }
        _selectedIdx = idx;
        if (_selectedIdx == -1) // Deselecting
        {
            _selectorObj.SetActive(false);
            CheckDestroyPlacementPreview();

            return;
        }
        // If the slot is not active
        if (!_hotbarSlots[_selectedIdx].transform.parent.gameObject.activeInHierarchy)
        {
            return;
        }
        OnSelectedIdxChanged.Invoke();
        _selectorObj.SetActive(true);
        _selectorObj.transform.position = HotbarSlots[idx].transform.position;
        SelectBlueprint();
    }
    #region KeyboardBindings
    public void SelectThroughKeyboardHotKey(int idx)
    {
        Select(idx);
        if (CurrentActivePlacer != null)
        {
            CurrentActivePlacer.AllowedToPlaceByOtherScripts = true;
        }
    }

    public void Select1(InputAction.CallbackContext context)
    {
        if(context.performed)
            SelectThroughKeyboardHotKey(0);
    }
    public void Select2(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectThroughKeyboardHotKey(1);
    }
    public void Select3(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectThroughKeyboardHotKey(2);
    }
    public void Select4(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectThroughKeyboardHotKey(3);
    }
    public void Select5(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectThroughKeyboardHotKey(4);
    }
    public void Select6(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectThroughKeyboardHotKey(5);
    }
    public void Select7(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectThroughKeyboardHotKey(6);
    }
    public void Select8(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectThroughKeyboardHotKey(7);
    }
    public void Select9(InputAction.CallbackContext context)
    {
        if (context.performed)
            SelectThroughKeyboardHotKey(8);
    } 
    #endregion
}
