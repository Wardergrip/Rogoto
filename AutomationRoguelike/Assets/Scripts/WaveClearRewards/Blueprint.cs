using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BluePrint")]
public class Blueprint : ScriptableObject
{
    [SerializeField] private PlayerStructure _structure;
    [SerializeField] private string _descriptionText;
    [SerializeField] private Sprite _icon;
    [SerializeField] private Sprite _gif;
    [SerializeField] private RuntimeAnimatorController _gifAnimationController;
    
    [SerializeField] private bool _isProcessingMachine;

    [SerializeField] private List<MonoVariant> _possibleVariants = new();

    public List<MonoVariant> PossibleVariants { get => _possibleVariants; }

    public string Name { get => _structure.Name; }
    public string DescriptionText { get => _descriptionText; }
    public Sprite Icon { get => _icon; }
    public bool IsProcessingMachine { get => _isProcessingMachine;  }
    public PlayerStructure Structure { get => _structure;  }
    public Sprite Gif { get => _gif; }
    public RuntimeAnimatorController GifAnimationController { get => _gifAnimationController;  }
    public MonoVariant Variant { get; set; } = null;

    public Blueprint Duplicate()
    {
        Blueprint blueprint = CreateInstance<Blueprint>();
        blueprint._structure = _structure;
        blueprint._descriptionText = _descriptionText;
        blueprint._icon = _icon;
        blueprint._gif = _gif;
        blueprint._gifAnimationController = _gifAnimationController;
        blueprint._isProcessingMachine = _isProcessingMachine;
        blueprint.Variant = Variant;
        return blueprint;
    }
}
