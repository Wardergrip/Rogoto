using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursorSetProxy : MonoBehaviour
{
    [SerializeField] CursorState _state;
    [SerializeField] bool _isAllowedToOverideAbsolute;
    [SerializeField] bool _isAllowedToBeOverriden;
    public void SetCursor()
    {
        CustomCursor.Instance.SetCursorState(_state,_isAllowedToOverideAbsolute,_isAllowedToBeOverriden);
    }
}
