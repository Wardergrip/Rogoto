using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

public class BasicTooltipShower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private List<string> _texts;
	[SerializeField] private float _showDelay = 0.5f;
	private bool _isShowingBecauseOfThis = false;

	public void OnPointerEnter(PointerEventData eventData)
	{
		StartCoroutine(ShowCoroutine());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Hide();
	}

	private void Show()
	{
		_isShowingBecauseOfThis = true;
		StringBuilder sb = new();
		_texts.ForEach(text => sb.AppendLine(text));
		BasicTooltip.Instance.UpdateText(sb.ToString());
		BasicTooltip.Instance.Show();
	}

	private void Hide()
	{
		_isShowingBecauseOfThis = false;
		StopAllCoroutines();
		BasicTooltip.Instance.Hide();
	}

	private IEnumerator ShowCoroutine() 
	{
		yield return new WaitForSeconds(_showDelay);
		Show();
	}

	private void OnDisable()
	{
		if (_isShowingBecauseOfThis)
			Hide();
	}
}
