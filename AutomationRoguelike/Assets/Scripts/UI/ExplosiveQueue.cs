using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Trivial;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ExplosiveQueue : MonoBehaviour
{
    [Tooltip("Put in correct order, first the bottom most preview")]
    [SerializeField] private Image[] _previews;
    [Tooltip("Collection of all the explosive presets")]
    [SerializeField] private Explosive[] _explosives;
    private List<Explosive> _currentQueue = new();
    private Explosive _currentExplosive;
    private int _currentExplosiveIndex;
    [SerializeField] private GameObject _explodeButton;
    private Stack<List<GameObject>> _disabledConnections = new();
    [SerializeField] private float _explodeTotalWaitTime = 0.1f;

    public UnityEvent OnExplosivePlace;
    public UnityEvent OnExplosiveUndo;
    public UnityEvent OnExplosiveRotate;
    public UnityEvent OnExplosiveMove;
    public UnityEvent OnExplosivePathComplete;

    public bool IsPathComplete { get; private set; } = false;

    public void ExplodePath()
    {
        CameraController cameraController = FindObjectOfType<CameraController>();
  		if (!IsPathComplete 
            || !GameSystem.s_AllowExplode 
            || (cameraController != null  && cameraController.IsCameraInCinematic))
        {
			return;
		}
        StartCoroutine(ExplodeCoroutine(() => GameSystem.Instance.PathExploded()));
        _explodeButton.SetActive(false);
        GameSystem.s_AllowExplode = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="waitTime">Negative means wait a frame</param>
    /// <returns></returns>
    private IEnumerator ExplodeCoroutine(Action callBack = null) 
    {
        Explosive.s_ActiveExplosives.ForEach(explosive => explosive.SetVisualVisibility(false));
        float waitTimePerShape = _explodeTotalWaitTime / Explosive.s_ActiveExplosives.Count;
		while (Explosive.s_ActiveExplosives.Count > 0)
		{
			Explosive.s_ActiveExplosives.First().Explode(waitTimePerShape);
		    yield return new WaitForSeconds(waitTimePerShape);
		}
		callBack?.Invoke();
	}

    private void OnEnable()
    {
        IsPathComplete = false;
        GenerateNewQueue();
		CameraController.NotifyExplosiveQueueVisibleState(true);
		_disabledConnections.Clear();
        _currentExplosive = Instantiate(_currentQueue[_currentExplosiveIndex], Vector3.zero, Quaternion.identity);
		_currentExplosive.OnMove.AddListener((Explosive explosive) => OnExplosiveMove?.Invoke());
	}
    private void OnDisable()
    {
		CameraController.NotifyExplosiveQueueVisibleState(false);

		if (_currentExplosive!=null)
        Destroy(_currentExplosive.gameObject);
    }
    private void UpdateCurrentExplosiveOnPlace()
    {
        if (_currentExplosive.IsConnectedToEnd)
        {
            _explodeButton.SetActive(true);
            OnExplosivePathComplete?.Invoke();
            IsPathComplete = true;
			_currentExplosive = null;
            return;
        }
        _currentExplosive = Instantiate(_currentQueue[_currentExplosiveIndex], Vector3.zero, Quaternion.identity);
        _currentExplosive.OnMove.AddListener((Explosive explosive) => OnExplosiveMove?.Invoke());
    }
    private void ReEnableLastConnections()
    {
        foreach (GameObject obj in _disabledConnections.Pop()) 
        {
            obj.SetActive(true);
        } 
    }
    private void DisableOldConnections()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("ExplosiveValidConnectionCollider");
        List<GameObject> actualObjs = new();
        foreach (GameObject @object in objs)
        {
            if (@object.transform.parent != _currentExplosive.transform&&@object.activeSelf==true)
            {
                actualObjs.Add(@object);
                @object.SetActive(false);
            }
        }
        _disabledConnections.Push(actualObjs);
    }
    private void UpdateCurrentExplosiveOnUndo()
    {
        //check if last explosive made a full connection
        if (_currentExplosive == null)
        {
            _explodeButton.SetActive(false);
        }
        else
        Destroy(_currentExplosive.gameObject);
        Destroy(Explosive.s_ActiveExplosives[_currentExplosiveIndex].gameObject);
        Explosive.s_ActiveExplosives.Remove(Explosive.s_ActiveExplosives[_currentExplosiveIndex]);
        _currentExplosive = Instantiate(_currentQueue[_currentExplosiveIndex], Vector3.zero, Quaternion.identity);
		_currentExplosive.OnMove.AddListener((Explosive explosive) => OnExplosiveMove?.Invoke());
		ReEnableLastConnections();
    }
    public void Rotate(InputAction.CallbackContext context)
    {
        if (CameraController.s_OverrideZoomAllow)
        {
            return;
        }

        if (context.performed)
        {
            float scrollAxis = context.ReadValue<Vector2>().y;
            if (_currentExplosive != null)
            {
                _currentExplosive.Rotate(scrollAxis);
                OnExplosiveRotate?.Invoke();
            }
		}
    }
	public void RotateKeyboard(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			if (_currentExplosive != null)
			{
				_currentExplosive.Rotate(1);
				OnExplosiveRotate?.Invoke();
			}
		}
	}
	public void Place(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            PlaceDownExplosive();
			OnExplosivePlace?.Invoke();
		}
    }
    public void Undo(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            UndoPlacement();
            OnExplosiveUndo?.Invoke();
		}
    }
    private void PlaceDownExplosive()
    {
        if (_currentExplosive == null)
            return;
        if (!_currentExplosive.TryPlace())
        {
            return;
        }
        DisableOldConnections();
        ++_currentExplosiveIndex;
        if (_currentExplosiveIndex + _previews.Length > _currentQueue.Count)
        {
            AddNewExplosiveToQueue();
        }
        SetPreviews();
        UpdateCurrentExplosiveOnPlace();
    }
    private void UndoPlacement()
    {
        if (_currentExplosiveIndex <= 0)
            return;
		--_currentExplosiveIndex;
        SetPreviews();
        UpdateCurrentExplosiveOnUndo();
        IsPathComplete = false;
    }
    private void GenerateNewQueue()
    {
        _currentExplosiveIndex = 0;
        _currentQueue.Clear();
        for (int i = 0; i < _previews.Length; i++)
        {
            AddNewExplosiveToQueue();
        }
        SetPreviews();
    }
    private void SetPreviews()
    {
        for (int i = 0; i < _previews.Length; i++)
        {
            _previews[i].sprite = _currentQueue[i + _currentExplosiveIndex].PreviewIcon;
        }
    }
    private void AddNewExplosiveToQueue()
    {
        int randomExplosive = UnityEngine.Random.Range(0, _explosives.Length);
        _currentQueue.Add(_explosives[randomExplosive]);
    }
}
