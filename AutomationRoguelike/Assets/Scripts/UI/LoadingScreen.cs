using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoadingScreen : MonoSingleton<LoadingScreen>
{
    [SerializeField] private List<Image> _images = new();
    [SerializeField] private List<TextMeshProUGUI> _tmpros = new();

	private float _previousAlpha = 0f;

	public UnityEvent OnVisible;
	public UnityEvent OnInvisible;

	protected override void LateAwake()
	{
		SetAlpha(0.0f);
	}

	public void SetAlpha(float alpha)
    {
        Color intermediate = Color.white;
        _images.ForEach(image => 
        {
			intermediate = image.color;
			intermediate.a = alpha;
			image.color = intermediate; 
        });
        _tmpros.ForEach(tmpro =>
        {
			intermediate = tmpro.color;
			intermediate.a = alpha;
			tmpro.color = intermediate;
		});

		if (_previousAlpha <= 0.0f && alpha > 0.0f)
		{
			OnVisible?.Invoke();
		}
		else if (_previousAlpha > 0.0f && alpha <= 0.0f) 
		{
			OnInvisible?.Invoke();
		}

		_previousAlpha = alpha;
    }
}
