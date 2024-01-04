using UnityEngine;

public abstract class MonoVariant : MonoBehaviour
{
    [SerializeField] private Sprite _sprite;
    [SerializeField] private string _description;
    [SerializeField] private Sprite _worldIcon;
    public abstract void Setup(PlayerStructure turret);
    public abstract bool IsTurret { get; }
    public Sprite Sprite { get => _sprite;}
    public string Description { get => _description; }
    public Sprite WorldIcon { get => _worldIcon;}
}