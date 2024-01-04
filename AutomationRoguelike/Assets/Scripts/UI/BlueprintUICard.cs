using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Trivial;


public class BlueprintUICard : MonoBehaviour
{
    private enum BlueprintType
    {
        Turret, Trap, Machine
    }
    [SerializeField] private BlueprintType _type;
    private Dictionary<BlueprintType, Blueprint[]> _blueprints;
    [SerializeField] private Blueprint[] _turrets;
    [SerializeField] private Blueprint[] _traps;
    [SerializeField] private Blueprint[] _processingMachines;
    [SerializeField] private TextMeshProUGUI _blueprintNameText;
    [SerializeField] private GameObject _variantBox;
    [SerializeField] private Image _variantImage;
    [SerializeField] private GameObject _variantDescriptionBox;
    [SerializeField] private TextMeshProUGUI _variantDescription;
    private BlueprintScreen _screen;
    private Vector2 _startPosition;
    private Transform _target;
    private Blueprint _pickedBlueprint;
    private Coroutine _movingCouroutine;
    private static readonly List<BlueprintUICard> _shownCards = new();
    private static readonly float s_weightToGetVariant = 0.4f; // Bigger number is more chance, other weight is 1.0f
    public Blueprint PickedBlueprint { get => _pickedBlueprint; }
    private bool _inserted;
    private bool _nameRevealed;
    private float _lerpDuration = 0.3f;
    private void Insert()
    {
        _inserted = true;
        _target = _screen.InsertPosition;
        if (_movingCouroutine != null)
            StopCoroutine(_movingCouroutine);
        _movingCouroutine= StartCoroutine( transform.LerpToTarget(_target,_lerpDuration,()=>
        {
            _screen.ShowInfo(this);
            RevealName();
            transform.rotation = _target.rotation;
            transform.parent = _target;
            
        }));
        _screen.EjectInserted();
        _screen.InsertedCard = this;
        
    }
    public void Click()
    {
        if (_inserted)
        {
            _screen.EjectInserted();
           
        }
        else
        {
            Insert();
        }
    }
    public void Eject()
    {
        _inserted = false;
        if (_movingCouroutine != null)
            StopCoroutine(_movingCouroutine);
        transform.rotation = Quaternion.identity;
        transform.parent = _screen.transform;
        _movingCouroutine = StartCoroutine(transform.LerpToPosition(_startPosition, _lerpDuration));
        
    }
    private void RevealName()
    {
        if (_nameRevealed)
            return;
        _nameRevealed = true;
        _blueprintNameText.text = _pickedBlueprint.Name;
        GetComponent<TextTyper>().StartTypingEffect();
    }
    public void RevealVariant()
    {
        if (_pickedBlueprint.Variant == null)
            return;
        _variantBox.SetActive(true);
        _variantImage.sprite = _pickedBlueprint.Variant.Sprite;
        _variantImage.SetNativeSize();
        _variantDescription.text = _pickedBlueprint.Variant.Description;

    }
    public void ShowVariantDescription()
    {
        _variantDescriptionBox.SetActive(true);
        _variantDescription.text = _pickedBlueprint.Variant.Description;
    }

    private void Awake()
    {
        _blueprints = new()
        {
            {BlueprintType.Turret,_turrets },
            {BlueprintType.Trap, _traps },
            {BlueprintType.Machine, _processingMachines }
        };
        _shownCards.Add(this);
        _screen = transform.parent.GetComponent<BlueprintScreen>();
        _startPosition = transform.position;
        _pickedBlueprint = PickRandomBlueprint();
    }


    private bool CheckIfStructureAlreadyPicked(Blueprint pick)
    {

        foreach (BlueprintUICard blueprintCard in _shownCards)
        {
            if (blueprintCard != this && blueprintCard.PickedBlueprint.Name.Equals(pick.Name))
                return true;
        }
        return false;
    }
    public void SetNewStructure()
    {
        _pickedBlueprint = PickRandomBlueprint();
        
    } 
    
    private Blueprint PickRandomBlueprint()
    {
        Blueprint[] blueprints;
        _blueprints.TryGetValue(_type,out blueprints);
        Blueprint pick = blueprints[Random.Range(0, blueprints.Length)];

            if (pick.PossibleVariants.Count > 0 && RandomUtils.YesOrNo(s_weightToGetVariant+GameSystem.Instance.Luck*0.01f))
            {
                pick = GetVariant(pick);
            }
        //if (CheckIfStructureAlreadyPicked(pick))
        //    return PickRandomBlueprint(isMachine);
        //else
            return pick;

    }
    public void ChooseThisBluePrint()
    {
        
    }
    private Blueprint GetVariant(Blueprint blueprint)
    {
        Blueprint variantBlueprint = blueprint.Duplicate();
        variantBlueprint.Variant = blueprint.PossibleVariants.GetRandomValue();
        return variantBlueprint;
    }
    private void OnDestroy()
    {
        _shownCards.Remove(this);
    }

    
}
