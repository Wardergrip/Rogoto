using TMPro;
using UnityEngine;

public class BasicTooltip : MonoStatic<BasicTooltip>
{
    [SerializeField] private TextMeshProUGUI _tmpro;
    [SerializeField] private RectTransform _background;
    [SerializeField] private Vector2 _padding;
    private RectTransform _canvasTransform;
    private RectTransform _rectTransform;

	protected override void LateAwake()
	{
        _rectTransform = GetComponent<RectTransform>();

        Canvas canvas = GetComponentInParent<Canvas>();
        Debug.Assert(canvas != null);
        _canvasTransform = canvas.GetComponent<RectTransform>();

        Hide();
	}

	public void UpdateText(string text)
    {
        _tmpro.SetText(text);
        _tmpro.ForceMeshUpdate(true);

        Vector2 sizeDelta = _tmpro.GetRenderedValues(false);
        _background.sizeDelta = sizeDelta + _padding;
    }

	private void Update()
	{
		// https://www.youtube.com/watch?v=YUIohCXt_pc

		Vector2 anchoredPos = Input.mousePosition / _canvasTransform.localScale.x;
        anchoredPos.x = Mathf.Clamp(anchoredPos.x, 0, _canvasTransform.rect.width - _background.rect.width);
        anchoredPos.y = Mathf.Clamp(anchoredPos.y, 0, _canvasTransform.rect.height - _background.rect.height);
		_rectTransform.anchoredPosition = anchoredPos;
	}

    public void Show()
    {
        _tmpro.gameObject.SetActive(true);
        _background.gameObject.SetActive(true);
    }

    public void Hide()
    {
		_tmpro.gameObject.SetActive(false);
		_background.gameObject.SetActive(false);
	}
}
