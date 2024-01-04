
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct DebuffIcon
{
    public Sprite Icon;
    public string Identifier;
}
public struct DebuffIconObject
{
    public Image Icon;
    public TextMeshProUGUI StackCounter;

    public DebuffIconObject(Image icon, TextMeshProUGUI stackCounter)
    {
        Icon = icon;
        StackCounter = stackCounter;
    }
}
public class DebuffShower : MonoBehaviour
{
    private HediffHandler _handler;
    [SerializeField] private List<DebuffIcon> _iconList = new();
    [SerializeField] private Image _debuffIconPreFab;
    private Dictionary<string, DebuffIconObject> _currentShowingDebuffs = new();

    public HediffHandler Handler { get => _handler; set => _handler = value; }

    public void SetUp()
    {
        _handler.OnHediffUpdated.AddListener(ShowDebuff);
        _handler.OnHediffRemoved.AddListener(RemoveShowDebuff);
    }
    private void ShowDebuff(Hediff hediff)
    {
        if(_iconList.Where(icon => icon.Identifier == hediff.Identifier).Count()<1)
        {
            Debug.LogWarning("No matching icon found for debuff!");
            return;
        }
        if (_currentShowingDebuffs.TryGetValue(hediff.Identifier, out DebuffIconObject icon))
        {
            if (hediff.IsStackable)
            {
                icon.StackCounter.text = hediff.Stacks.ToString();
            }
        }
        else
        {
        Image iconSpawned= Instantiate(_debuffIconPreFab, transform);
        iconSpawned.sprite = _iconList.Where(icon => icon.Identifier.Equals(hediff.Identifier)).FirstOrDefault().Icon;
        TextMeshProUGUI text = iconSpawned.gameObject.GetComponentInChildren<TextMeshProUGUI>();
            if (hediff.IsStackable)
            {
                text.gameObject.SetActive(true);
                text.text = hediff.Stacks.ToString();
            }
            else
            {
                text.gameObject.SetActive(false);
            }
            DebuffIconObject iconObj = new DebuffIconObject(iconSpawned,text);
        _currentShowingDebuffs.Add(hediff.Identifier, iconObj);
        }
    }
    private void RemoveShowDebuff(Hediff hediff)
    {
        if (_currentShowingDebuffs.TryGetValue(hediff.Identifier, out DebuffIconObject icon))
        {
            _currentShowingDebuffs.Remove(hediff.Identifier);
            Destroy(icon.Icon.gameObject);
        }
    }
    private void OnDestroy()
    {
        if(_handler!=null)
            _handler.OnHediffUpdated.RemoveListener(ShowDebuff);
    }
}
