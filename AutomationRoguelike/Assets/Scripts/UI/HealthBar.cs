using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Transform _objectImAttachedTo;
    [SerializeField] private Health _health;
    [SerializeField] private DebuffShower _debuffShower;
    [SerializeField] private Color _barColor;
    private int _level;
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _healthBarBackGround;
    [SerializeField] private Image _seperationsBar;
    [SerializeField] private Image _seperatorImage;
    private readonly List<Image> _seperatorImages = new();
    [SerializeField] private TextMeshProUGUI _levelCounter;
    [SerializeField] private int _seperationsInHealthBar;
    public int SeperationsInHealthBar
    {
        get => _seperationsInHealthBar;
        set
        {
            _seperationsInHealthBar = value;
            _seperatorImages.ForEach(x => Destroy(x.gameObject));
            _seperatorImages.Clear();
            SetUpSeperation();
        }
    }

    public DebuffShower DebuffShower { get => _debuffShower;  }

    private void Start()
    {
        _health.OnTookDamage.AddListener(UpdateHealthBar);
        _health.OnGainedHealth.AddListener(UpdateHealthBar);
        _health.OnDied.AddListener(ObjectAttachedToDies);
        
        _healthBar.color = _barColor;
        _levelCounter.text = _level.ToString();
        SetUpSeperation();
        UpdateHealthBar();
    }
    private void SetUpSeperation()
    {
        for (int i = 0; i < _seperationsInHealthBar; i++)
        {
            float seperatorPlacement = (_seperationsBar.rectTransform.sizeDelta.x / (float)(_seperationsInHealthBar + 1)) * (i+1);
            Image seperator = Instantiate(_seperatorImage, _seperationsBar.transform);
            seperator.rectTransform.localPosition=new Vector3(seperatorPlacement,0,0);
            Vector2 seperatorSizeDelta = seperator.rectTransform.sizeDelta;
            seperatorSizeDelta.y = _seperationsBar.rectTransform.sizeDelta.y;
            seperator.rectTransform.sizeDelta = seperatorSizeDelta;
            _seperatorImages.Add(seperator);
        }
    }
    private void Update()
    {
        if(_objectImAttachedTo!=null)
        transform.position = _objectImAttachedTo.transform.position;
        //transform.LookAt(Camera.main.transform);
    }
    public void SetUpHealthBar(Transform objectImAttachedTo, Health health , Color barColor, int level)
    {
        _objectImAttachedTo = objectImAttachedTo;
        _health = health;
        _barColor = barColor;
        _level = level;
    }
	public void SetLevel(int level)
	{
		_level = level;
		_levelCounter.text = _level.ToString();
	}
	private void UpdateHealthBar(int value = -1)
    {
        float currentHealthPercentage = (float)_health.HealthAmount / (float)_health.MaxHealth;
        _healthBar.fillAmount = currentHealthPercentage;
    }
    private void ObjectAttachedToDies()
    {
        Destroy(this.gameObject);
    }
}
