using TMPro;
using UnityEngine;

public enum EffectStatIcon
{
    Luck, Damage, Gold, Enemy, Base
}
public class Effect : MonoBehaviour
{
    
    [SerializeField] EffectStatIcon _effectStatIcon;
    [SerializeField] private string _name;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private Sprite _icon;

    public Sprite Icon { get => _icon; }
    public TextMeshProUGUI Description { get => _description;  }
    public string Name { get => _name;  }
    public EffectStatIcon EffectStatIcon { get => _effectStatIcon;  }

    public virtual void Init(EffectRarity rarity )
    {

    }
    public virtual void Excecute()
    {

    }
    public virtual bool IsAvailable()
    {
        return true;
    }
    public virtual string OverrideDescription()
    {
        return Description.text;
    }
}
