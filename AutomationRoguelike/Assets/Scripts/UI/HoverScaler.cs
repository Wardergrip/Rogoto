using UnityEngine;
using UnityEngine.EventSystems;

public class HoverScaler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private Vector3 _hoverScale = Vector3.one;
	private Vector3 _originalScale;
	[SerializeField] private Vector3 _rotate = Vector3.zero;
	private Vector3 _originalRotation;

	private void Awake()
	{
		_originalScale = transform.localScale;
		_originalRotation = transform.eulerAngles;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Enter();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Exit();	
	}

	public void Enter()
	{
		transform.localScale = _hoverScale;
		transform.Rotate(_rotate);
	}

	public void Exit() 
	{
		transform.Rotate(-_rotate);
		transform.localScale = _originalScale;
	}

	private void OnDisable()
	{
		transform.eulerAngles = _originalRotation;
		transform.localScale = _originalScale;
	}
}
