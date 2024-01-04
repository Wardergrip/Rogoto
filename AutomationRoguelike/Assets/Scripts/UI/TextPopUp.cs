using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextPopUp : MonoBehaviour
{
    [SerializeField] private Canvas _textPreFab;
    [SerializeField] private float _alphaReductionSpeed = 1f;
    [SerializeField] private Vector2 _moveTextDirection= new Vector3(1,1);
    [SerializeField] private float _moveTextSpeed=10f;
    [SerializeField] private string _text;
    [SerializeField] private Color _textColor;
    private TextMeshProUGUI _spawnedText;
    public void ShowPopUp()
    {
        CheckIfAlreadyPoppedUp();
       _spawnedText= Instantiate(_textPreFab, Vector3.zero, Quaternion.identity).GetComponentInChildren<TextMeshProUGUI>();
        _spawnedText.rectTransform.anchoredPosition = Input.mousePosition;
        _spawnedText.text = _text;
        _spawnedText.color = _textColor;
        StartCoroutine(UpdateText());
    }
    private void CheckIfAlreadyPoppedUp()
    {
        if (_spawnedText != null)
        {
            StopAllCoroutines();
            Destroy(_spawnedText.transform.parent.gameObject);
        }
            
    }
    private void OnDestroy()
    {
        CheckIfAlreadyPoppedUp();
    }

    private IEnumerator UpdateText()
    {
        _spawnedText.alpha = 1;
        while (_spawnedText.alpha > 0)
        {
            _spawnedText.rectTransform.anchoredPosition+= _moveTextDirection * _moveTextSpeed * Time.deltaTime;
            _spawnedText.alpha -= _alphaReductionSpeed*Time.deltaTime;
            yield return null;
        }
        Destroy(_spawnedText.transform.parent.gameObject);
    }
    
}
