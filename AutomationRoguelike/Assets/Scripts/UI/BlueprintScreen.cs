using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class BlueprintScreen : MonoBehaviour
{
    [Header("Vars")]
    [SerializeField] private GameObject _hotbarPopout;
    [SerializeField] private HotbarMimic _mimic;
    [SerializeField] private GameObject _skipButton;
    [SerializeField] private GameObject _selectButton;
    [SerializeField] private Image _gifImage;
    [SerializeField] private Animator _gifAnimator;
    [SerializeField] private GameObject _gifBox;
    [SerializeField] private GameObject _statBox;
    [SerializeField] private TextMeshProUGUI _statText;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private RectTransform _insertPosition;
    private ShipWreckLoot _wreck;
    private BlueprintUICard _insertedCard;
    private CanvasManager _hud;
    public RectTransform InsertPosition { get => _insertPosition; }
    public BlueprintUICard InsertedCard { get => _insertedCard; set => _insertedCard = value; }

    [Header("Events")]
    public UnityEvent OnEject;
    public UnityEvent OnInsert;
    public UnityEvent OnStatsShowed;

    public void EjectInserted()
    {
        if (_insertedCard != null)
        {
            _insertedCard.Eject ();
			OnEject?.Invoke();
			_insertedCard = null;
            ResetInfo();
            _hotbarPopout.SetActive(false);
        }
    }
    public void Select()
    {
        _statBox.SetActive(false);
        _gifBox.SetActive(false);
        _hotbarPopout.SetActive(true);
        if (_insertedCard.PickedBlueprint.IsProcessingMachine)
        {
            _mimic.SetUp(Hotbar.s_Hotbars.Where(x => x.HotbarType == HotbarType.Machine).First(), _insertedCard.PickedBlueprint);
        }
        else
        {
            _mimic.SetUp(Hotbar.s_Hotbars.Where(x => x.HotbarType == HotbarType.Combat).First(), _insertedCard.PickedBlueprint);
        }
    }
    private void Awake()
    {
        _wreck = transform.parent.GetComponent<ShipWreckLoot>();
        _hud = FindObjectOfType<CanvasManager>();
        _hud.HideHud();
    }
    public void Skip()
    {
        BlueprintChosen();
        _hud.SHowHud();
    }

    private void BlueprintChosen()
    {
        _wreck.BlueprintChosen();
        Destroy(this.gameObject);
    }

    public void ShowInfo(BlueprintUICard card)
    {
        _skipButton.SetActive(false);
        _selectButton.SetActive(true);
        _descriptionText.text = card.PickedBlueprint.DescriptionText;
        _descriptionText.gameObject.SetActive(false);
        _iconImage.transform.parent.gameObject.SetActive(true);
        _iconImage.transform.parent.GetComponent<Animator>().SetTrigger("Entry");
        _iconImage.sprite = card.PickedBlueprint.Icon;
        _gifBox.SetActive(false);
        _statBox.SetActive(false);
        OnInsert?.Invoke();
    }
    private void ResetInfo()
    {
        _skipButton.SetActive(true);
        _selectButton.SetActive(false);
        _descriptionText.gameObject.SetActive(false);
        _iconImage.transform.parent.gameObject.SetActive(false);
        _gifBox.SetActive(false);
        _gifBox.GetComponent<Animator>().SetTrigger("Cancel");
        _statBox.SetActive(false);
        _statBox.GetComponent<Animator>().SetTrigger("Cancel");
        _iconImage.transform.parent.GetComponent<Animator>().SetTrigger("Cancel");
    }
    public void SetGif()
    {
        if (_insertedCard == null)
            return;
        if (_insertedCard.PickedBlueprint.Gif != null)
        {
            _gifBox.SetActive(true);
            _gifBox.GetComponent<Animator>().SetTrigger("Entry");
            _gifBox.GetComponent<Animator>().Update(Time.deltaTime);
            _gifImage.sprite = _insertedCard.PickedBlueprint.Gif;
            _gifAnimator.runtimeAnimatorController = _insertedCard.PickedBlueprint.GifAnimationController;
        }
 
    }

    public void SetStats()
    {
        if (_insertedCard == null)
            return;
        _statBox.SetActive(true);
        string statString = null;
        foreach (Stat stat in _insertedCard.PickedBlueprint.Structure.Stats)
        {
            statString += $"<sprite name={stat.GetName()}> {stat.FinalValue}\n\n";
        }
        statString += $"Cost ${_insertedCard.PickedBlueprint.Structure.Cost}";
        _statText.text = statString;
        OnStatsShowed?.Invoke();
    }
}
