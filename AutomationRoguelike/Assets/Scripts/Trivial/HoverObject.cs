using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoverObject : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private UnityEvent _onEnterObject;
	[SerializeField] private UnityEvent _onExitObject;
	[SerializeField] private UnityEvent _onStayObject;
	private bool _pointerIsOver;
	public void OnPointerEnter(PointerEventData eventData)
	{
		_onEnterObject.Invoke();
		_pointerIsOver = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		_onExitObject.Invoke();
		_pointerIsOver = false;
	}
	private void Update()
	{
		if (_pointerIsOver)
			_onStayObject.Invoke();
	}
}
