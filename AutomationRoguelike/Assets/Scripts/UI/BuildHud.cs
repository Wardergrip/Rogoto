using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildHud : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Hotbar[] _hotBars;
    [SerializeField] GameObject _demolishBar;
    [SerializeField] GameObject _demolishProgressBar;
    public static PlayerStructure HoveredStructure { get; set; }
    public static ConveyerBeltLine s_ActiveConveyerLine { get => s_activeConveyerLine; set => s_activeConveyerLine = value; }

    private static GameObject s_hoverConveyerStart;
    public static GameObject S_HoverConveyerEnd;
    private PlayerStructure _currentDemolishStructure;
    private bool _demolishKeyHeld = false;
    [SerializeField] ConveyerBeltLine _conveyerLinePreFab;
    private static ConveyerBeltLine s_activeConveyerLine;

    public static void CheckIfDestroyedHoverConveyerStart(GameObject obj)
    {
        if (s_hoverConveyerStart == obj)
        {
            s_hoverConveyerStart = null;
        }
        
    }
    public static void SetHoverConveyerEnd(GameObject obj)
    {
        if (S_HoverConveyerEnd == obj)
        {
            S_HoverConveyerEnd = null;
        }
        else
        {
            S_HoverConveyerEnd = obj;
        }

    }
    public static void ResetHoverConveyerConnections()
    {
        S_HoverConveyerEnd = null;
        s_hoverConveyerStart = null;
    }
    public static void SetHoverConveyerStart(GameObject obj)
    {
        if (s_hoverConveyerStart==obj)
        {
            s_hoverConveyerStart = null;
        }
        else
        {
            s_hoverConveyerStart = obj;
        }
        
    }
    private void OnDisable()
    {
        HoveredStructure = null;
    }
    private void Update()
    {
        if (!_demolishKeyHeld)
            return;
        else
        {
            CustomCursor.Instance.SetCursorState(CursorState.Demolishing, false, true);
        }
        if (HoveredStructure == null)
        {
            StopAllCoroutines();
            _demolishBar.SetActive(false);
            _currentDemolishStructure = null;
            return;
        }
        if (_currentDemolishStructure == null || _currentDemolishStructure != HoveredStructure)
        {
            StopAllCoroutines();
            StartCoroutine(DemolishStructure());
        }
    }
    public void Select(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (s_hoverConveyerStart)
            {
                StartConveyerLine();
                S_HoverConveyerEnd = null;
                return;
            }
            if (HoveredStructure)
            {
                HoveredStructure.ShowStatsMenu();
            }
            
        }
        else if (context.canceled)
        {
            if (s_ActiveConveyerLine!=null)
            {
                s_ActiveConveyerLine.TryPlace();           
            }
        }
        

    }
    private void StartConveyerLine()
    {
        if (Hotbar.CurrentActivePlacer != null)
            return;
       s_ActiveConveyerLine= Instantiate(_conveyerLinePreFab, s_hoverConveyerStart.transform.position, s_hoverConveyerStart.transform.parent.rotation);
       
    }
    public void Deselect(InputAction.CallbackContext context)
    {
        if (HoveredStructure)
        {
            HoveredStructure.HideStatsMenu();
        }
    }
    public void Demolish(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _demolishKeyHeld = true;

        }
        if (context.canceled)
        {
            _demolishBar.SetActive(false);
            _currentDemolishStructure = null;
            StopAllCoroutines();
            _demolishKeyHeld = false;
            CustomCursor.Instance.SetCursorState(CursorState.Default, false, true);
        }
    }
    private IEnumerator DemolishStructure()
    {
        float demolishTime = 1f;
        float maxDemolishTime = demolishTime;
        _currentDemolishStructure = HoveredStructure;
        _demolishBar.SetActive(true);
        _demolishBar.transform.position = Camera.main.WorldToScreenPoint(HoveredStructure.transform.position);
        while (demolishTime>0)
        {
            CustomCursor.Instance.SetCursorState(CursorState.Demolishing, false, true);
            float currentDemolishProgress =1- demolishTime / maxDemolishTime ;
            _demolishProgressBar.transform.localScale = new Vector3(currentDemolishProgress, 1, 1);
            demolishTime -= Time.deltaTime;
            yield return null;
        }
        _demolishBar.SetActive(false);
        HoveredStructure.StartDestroy();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        foreach (Hotbar hotbar in _hotBars)
        {
            hotbar.Deselect();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if(Hotbar.CurrentActivePlacer!=null) 
            Hotbar.CurrentActivePlacer.AllowedToPlaceByOtherScripts = true;
    }

    void Start()
    {
        _hotBars = transform.GetComponentsInChildren<Hotbar>();
    }   
}
