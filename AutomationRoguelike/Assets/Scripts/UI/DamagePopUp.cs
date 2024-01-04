using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Trivial.DestroyTimer))]
public class DamagePopUp : MonoBehaviour
{
    [System.Serializable]
    private struct NumberScalePair
    {
        public int Number;
        public Vector3 Scale;
    }
    [Header("Animation")]
    [SerializeField] private RectTransform _objToMove;
    [SerializeField] private float _initialForce;
    private Vector2 NewInitialForce 
    { 
        get
        {
            float randomAngle = Random.Range(0, Mathf.PI);
            return new(_initialForce * Mathf.Cos(randomAngle),_initialForce * Mathf.Sin(randomAngle));
        }
    }
    [SerializeField] private Vector3 _minGravity;
    [SerializeField] private Vector3 _maxGravity;
    private Vector2 NewGravity { get => Vector3.Lerp(_minGravity, _maxGravity, Random.value); }
    [SerializeField] private Trivial.DestroyTimer _destroyTimer;
    [SerializeField] private float _minDuration = 1.0f;
    [SerializeField] private float _maxDuration = 1.0f;
    private float NewDuration { get => Mathf.Lerp(_minDuration, _maxDuration, Random.value); }

    [Header("Damage")]
    [SerializeField] private TextMeshProUGUI _damageText;
    [SerializeField] private Color _colorToLerpTo = Color.white;
    [SerializeField] private Vector3 _scaleToLerpTo = Vector3.one;
    [SerializeField] private NumberScalePair _minDamageScale;
    [SerializeField] private NumberScalePair _maxDamageScale;
    private int _damageNumber = 1;
    private NumberScalePair NewDamageScale
    {
        get
        {
            float t = Mathf.InverseLerp(_minDamageScale.Number,_maxDamageScale.Number, Mathf.Clamp(_damageNumber,_minDamageScale.Number,_maxDamageScale.Number));
            return new()
            {
                Number = (int)Mathf.Lerp(_minDamageScale.Number, _maxDamageScale.Number, t),
                Scale = Vector3.Lerp(_minDamageScale.Scale, _maxDamageScale.Scale, t)
            }; ;
        }
    }

    public void SetNumberText(int number, string prefix = "", string postfix = "")
    {
        _damageText.text = $"{prefix}{number}{postfix}";
        _damageNumber = number;
    }

    public void SetColor(Color color) 
    {
        _damageText.color = color;
        //color.a = 0;
		_colorToLerpTo = color;
	}

    private void Awake()
    {
        if (_destroyTimer != null)
        {
            _destroyTimer = GetComponent<Trivial.DestroyTimer>();
        }
        Debug.Assert(!_destroyTimer.RunOnStart);
        _destroyTimer.Time = int.MaxValue;
    }

    private void Start()
    {
        StartCoroutine(PopUpCoroutine());
    }

    private IEnumerator PopUpCoroutine()
    {
        float duration = NewDuration;
        _destroyTimer.Time = duration + 0.5f;
        _destroyTimer.StartTimer();
        float elapsedSec = 0.0f;

        Vector3 gravity = NewGravity;
        Vector3 force = NewInitialForce;

        Color originalColor = _damageText.color;
        Vector3 originalScale = NewDamageScale.Scale;

        while (elapsedSec < duration) 
        {
            yield return null;
            elapsedSec += Time.deltaTime;

            // Position
            _objToMove.localPosition += force * Time.deltaTime;
            force += gravity * Time.deltaTime;
            
            // Lerps
            float t = elapsedSec / duration;
            _damageText.color = Color.Lerp(originalColor, _colorToLerpTo, t);
            _objToMove.transform.localScale = Vector3.Lerp(originalScale, _scaleToLerpTo, t);
        }
    }
}
