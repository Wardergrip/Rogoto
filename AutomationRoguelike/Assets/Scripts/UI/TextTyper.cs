using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class TextTyper : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _textElement;
    [SerializeField] float _timePerLetter=0.01f;
    private TMP_TextInfo _textInfo;
    private int _currentVisibleCharacterIndex;
    public UnityEvent OnTextFullyTyped;
    public UnityEvent OnLetterTyped;
    
    
    public void StartTypingEffect()
    {
        StopAllCoroutines();
        _currentVisibleCharacterIndex = 0;
        _textInfo = _textElement.textInfo;
        _textElement.maxVisibleCharacters = 0;
        StartCoroutine(TypeText());
    }
    private IEnumerator TypeText()
    {
        bool runOnce=false;
        while (_currentVisibleCharacterIndex<_textInfo.characterCount||!runOnce)
        {
            runOnce = true;
            char character = _textInfo.characterInfo[_currentVisibleCharacterIndex].character;
            _textElement.maxVisibleCharacters++;
            OnLetterTyped.Invoke();
            yield return new WaitForSeconds(_timePerLetter);

            _currentVisibleCharacterIndex++;
        }
        OnTextFullyTyped.Invoke();
    }

    
}
