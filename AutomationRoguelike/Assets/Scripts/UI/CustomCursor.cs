using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CursorImage
{
   [SerializeField] private Texture2D _texture;
   [SerializeField] private CursorState _state;
   [SerializeField] private Vector2 _offset;

    public CursorState State { get => _state; }
    public Texture2D Texture { get => _texture;  }
    public Vector2 Offset { get => _offset; }
}
public enum CursorState
{
    Default, Belt, Turret, Demolishing, Shipwreck, CombatReady, CombatWait
}
public class CustomCursor : MonoSingleton<CustomCursor>
{
    [SerializeField] private List<CursorImage> _images;
    private CursorState _state= CursorState.CombatWait;
    private bool _isAllowedToBeOverriddenLast= true;
    protected override void LateAwake()
    {
        SetCursorState(CursorState.Default, true, true);
    }
    public void SetCursorState(CursorState state, bool isAllowedToOverrideAbsolute,bool isAllowedToBeOverridden)
    {
        if (_state == state)
            return;
        if (!_isAllowedToBeOverriddenLast && !isAllowedToOverrideAbsolute)
        {
            return;
        }
        CursorImage image = _images.Find(x => x.State == state);
        if (image.Texture == null)
        {
            Debug.LogError("No valid texture for cursor!");
            return;
        }
        Cursor.SetCursor(image.Texture,image.Offset,CursorMode.Auto);
        _state = state;
        _isAllowedToBeOverriddenLast = isAllowedToBeOverridden;
    }
}
