using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PresentationPresenter : MonoBehaviour
{
    [SerializeField] private List<GameObject> _screens;
    private int _currentIdx = 0;

    public UnityEvent OnFinishedPresentation;

	private void Awake()
	{
		_screens.ForEach(screen => screen.SetActive(false));
	}

	private void OnEnable()
	{
		_currentIdx = 0;
		_screens[_currentIdx].SetActive(true);
	}

	public void Next(InputAction.CallbackContext context)
	{
		if (context.performed)
			Next();
	}

	public void Next()
    {
		_screens[_currentIdx].SetActive(false);
		++_currentIdx;
		if (_currentIdx == _screens.Count) 
		{ 
			OnFinishedPresentation?.Invoke();
			_currentIdx = -1;
			return;
		}
		_screens[_currentIdx].SetActive(true);
	}

	public void Previous(InputAction.CallbackContext context)
	{
		if (context.performed)
			Previous();
	}

	public void Previous() 
    {
		_screens[_currentIdx].SetActive(false);
		--_currentIdx;
		_currentIdx = Mathf.Clamp(_currentIdx, 0, _screens.Count - 1);
		_screens[_currentIdx].SetActive(true);
	}
}
