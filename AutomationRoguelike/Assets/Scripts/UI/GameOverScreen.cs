using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    [SerializeField] private RectTransform _screen;
    [SerializeField] private TextMeshProUGUI _statsText;
    [SerializeField] private float _fadeSpeed = 1.0f;
	private readonly List<Image> _images = new();
	private readonly List<TextMeshProUGUI> _texts = new();

	private void OnEnable()
    {
        SetAlpha(0.0f);
        _screen.gameObject.SetActive(false);
        _images.Clear();
        _texts.Clear();
		_images.AddRange(_screen.GetComponentsInChildren<Image>());
		_texts.AddRange(_screen.GetComponentsInChildren<TextMeshProUGUI>());
		GameSystem.OnShowGameOverScreen += GameSystem_OnShowGameOverScreen;
    }

    private void OnDisable()
    {
        GameSystem.OnShowGameOverScreen -= GameSystem_OnShowGameOverScreen;
    }

    private void GameSystem_OnShowGameOverScreen()
    {
        _statsText.text = $"Game over, you reached wave {GameSystem.Instance.WaveCounter}\r\nreport back to base";
        _screen.gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn(Action endOfFade = null)
    {
        for (float alpha = 0.0f;  alpha <= 1.0f; alpha += _fadeSpeed * Time.unscaledDeltaTime)
        {
            SetAlpha(alpha);
            yield return null;
        }
        SetAlpha(1.0f);
        endOfFade?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        Color intermediate = Color.white;
		_images.ForEach(image =>
		{
			intermediate = image.color;
			intermediate.a = alpha;
			image.color = intermediate;
		});
		_texts.ForEach(tmpro =>
		{
			intermediate = tmpro.color;
			intermediate.a = alpha;
			tmpro.color = intermediate;
		});
	}

    public void LoadMainMenu()
    {
        SceneLoader.Instance.LoadScene(SceneLoader.SceneRef.MainMenu);
    }
}
