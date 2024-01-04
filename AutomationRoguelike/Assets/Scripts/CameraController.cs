using MilkShake;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Trivial;
using System;

public class CinematicCommand
{
    private float _cinematicDuration;
    private float? _cinematicZoom;
    private Transform _cinematicTarget;

    public Transform CinematicTarget { get => _cinematicTarget; set => _cinematicTarget = value; }
    public float CinematicDuration { get => _cinematicDuration; set => _cinematicDuration = value; }
    public float? CinematicZoom { get => _cinematicZoom; set => _cinematicZoom = value; }
    public bool ShouldFollowTargetInstant { get; private set; }

    public CinematicCommand(Transform cinematicTarget, float cinematicDuration, float? cinematicZoom = null, bool shouldFollowTargetInstant = false)
    {
        CinematicTarget = cinematicTarget;
        CinematicDuration = cinematicDuration;
        CinematicZoom = cinematicZoom;
        ShouldFollowTargetInstant = shouldFollowTargetInstant;
    }
}

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public struct Borders
    {
        public Borders(float up = 100f, float down = 100f, float left = 100f, float right = 100f)
        {
            Up = up; Down = down; Left = left; Right = right;
        }

        public float Up;
        public float Down;
        public float Left;
        public float Right;
    }

    [SerializeField] private GameObject _cameraZoomChild;
    private static Shaker s_cameraShakeComponent;
    //uses the first list for asigning in inspector
    [SerializeField] private List<ShakePreset> _cameraShakePresets = new();
    private static List<ShakePreset> s_shakePresets;
    [SerializeField] private float _startZoom = 3;
    public UnityEvent OnStartCinematicEvent;
    public UnityEvent OnEndCinematicEvent;
    private void Update()
    {
        
        UpdateMovement();
        // If something is in the queue and we are not in cinematic mode, start cinematics.
        if (s_cinematicCommands.Count > 0 && (!_isCameraInCinematic))
        {
            FirstCinematicLogic();
            UpdateCinematicQueue();
        }
    }
    private void Awake()
    {
        s_cameraShakeComponent = transform.GetComponentInChildren<Shaker>();
        if (s_shakePresets == null)
        {
            s_shakePresets = _cameraShakePresets;

        }
        else
        {
            Debug.LogWarning("Multiple camera controllers in scene!!!!!!!! BAD");
        }
    }

    public static void ShakeCamera(int presetIndex)
    { 
        s_cameraShakeComponent.Shake(s_shakePresets[presetIndex]);
    }
    private void Start()
    {
        SetZoomAmount(_startZoom);
        GameSystem.OnRewardClaimed += this.EnableScript;
        GameSystem.OnNewCaveSelected += SetZoomForExplosivePath;
	}
    private void OnDestroy()
    { 
        GameSystem.OnRewardClaimed -= this.EnableScript;
        GameSystem.OnNewCaveSelected -= SetZoomForExplosivePath;
    }

    // Call EnqueueCinematic in any script to activate a cinematic
    #region CinematicLogic
    [Header("Cinematic")]
    private static readonly Queue<CinematicCommand> s_cinematicCommands = new();

    private Transform _target;
    private readonly float _cinematicLerpDuration = 1.5f;
    private readonly float _cinematicZoom = 2.5f;
    private float _cinematicTimer;
    private bool _isCameraInCinematic;
    public bool IsCameraInCinematic { get => _isCameraInCinematic; }
   
    public static void EnqueueCinematic(CinematicCommand command)
    {
        s_cinematicCommands.Enqueue(command);
    }
    private void FirstCinematicLogic()
    {
        if (_isCameraInCinematic)
            return;
        float cinematicZoom = s_cinematicCommands.Peek().CinematicZoom ?? _cinematicZoom;
		StartCoroutine(CinematicLerpToZoomAmount(cinematicZoom));
        OnStartCinematicEvent.Invoke();
    }
    private void UpdateCinematicQueue()
    {
        if(s_cinematicCommands.Count <= 0)
        {
            OnEndCinematicEvent.Invoke();
            return;
        }
        if (_isCameraInCinematic)
            return;

        CinematicCommand peekedCin = s_cinematicCommands.Peek();
        StartCoroutine(StartCinematic(peekedCin.CinematicDuration, peekedCin.CinematicTarget, peekedCin.CinematicZoom,  peekedCin.ShouldFollowTargetInstant));
    }
    private IEnumerator StartCinematic(float duration, Transform target, float? cinZoom,bool shouldFollowNotLerp)
    {
        _isCameraInCinematic = true;
        _target = target;
        _cinematicTimer = duration;
		float cinematicZoom = s_cinematicCommands.Peek().CinematicZoom ?? _cinematicZoom;
        if (cinematicZoom != _cinematicZoom) 
        {
            StartCoroutine(CinematicLerpToZoomAmount(cinematicZoom));
        }
		if (!shouldFollowNotLerp)
            StartCoroutine(transform.LerpToTarget(_target,_cinematicLerpDuration,null, true));
        else
            StartCoroutine(transform.FollowTarget(_target, true));
        while (_cinematicTimer > 0)
        {
            _cinematicTimer -= Time.deltaTime;
            yield return null;
        }
        _isCameraInCinematic = false;
        s_cinematicCommands.Dequeue();
        UpdateCinematicQueue();
    }

    #endregion
    #region Movement
    [Header("Movement")]
    [SerializeField] private Borders _borderSize = new();
    [SerializeField] private float _moveSpeed = 20;
    private Vector3 _movementDirection;
    private bool _cameraPanningActive = false;
    private bool _isGeneralCameraMovementActive = false;
    private Vector3 _lastPanPosition;
    private void UpdateMovement()
    {
        if (_isCameraInCinematic)
            return;
        CalculateCameraPanning();
        
        transform.Translate(_moveSpeed * Time.deltaTime * _movementDirection);
    }
    public void PanCamera(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            _cameraPanningActive = true;
            
            _lastPanPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _zoomAmount*_zoomMagnitude));
        }
        if (context.canceled)
        {
            _cameraPanningActive = false;
        }
    }
    private void CalculateBorderMovement()
    {
        if (_cameraPanningActive)
            return;
        CheckBorderMouseposition();
    }
    private void CalculateCameraPanning()
    {
        if (!_cameraPanningActive)
            return;
        
        Vector3 difference = _lastPanPosition - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _zoomAmount * _zoomMagnitude));
        
        transform.position += new Vector3(difference.x, 0,difference.z);
        _lastPanPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, _zoomAmount * _zoomMagnitude));
    }
    private void CheckBorderMouseposition()
    {
        Vector3 direction = Vector3.zero;
        if (Input.mousePosition.x < 0 + _borderSize.Left)
        {
            direction += new Vector3(-1, 0, 0);
        }
        if (Input.mousePosition.x > Screen.width - _borderSize.Right)
        {
            direction += new Vector3(1, 0, 0);
        }
        if (Input.mousePosition.y < 0 + _borderSize.Down)
        {
            direction += new Vector3(0, 0, -1);
        }
        if (Input.mousePosition.y > Screen.height - _borderSize.Up)
        {
            direction += new Vector3(0, 0, 1);
        }

        _movementDirection = direction.normalized;
    }
    public void MoveCamera(InputAction.CallbackContext context)
    {
        if (_cameraPanningActive) return;
        var vec = context.ReadValue<Vector2>();
        _movementDirection = new Vector3(vec.x,0,vec.y).normalized;

        _isGeneralCameraMovementActive = context.performed || _isGeneralCameraMovementActive;
        _isGeneralCameraMovementActive = !context.canceled && _isGeneralCameraMovementActive;
    }
    #endregion
    #region Zooming
    [Header("Zooming")]
    [SerializeField] private float _minZoom = 1f;
    [SerializeField] private float _maxZoom = 6f;
    private float _zoomAmount=2;
    private float _targetZoomAmount=2;
    private float _zoomMagnitude = 10;
    private float _zoomAxis;
    private float _zoomSpeed = 0.15f;
    private float _cinematicZoomLerpTimeElapsed;
    private float _cinematicZoomLerpDuration = 1;
    private float _cinematicZoomLerpSmoothing = 0.5f;
    private float _cinematicStartLerpZoomAmount;
    [SerializeField] private float _zoomLerpSmoothing = 0.5f;
    private static bool s_allowedToZoomWithScroll = true;
    public static bool s_OverrideZoomAllow { get; private set; } = false;

    public static void NotifyExplosiveQueueVisibleState(bool state)
    {
        s_allowedToZoomWithScroll = !state;
	}

    public static void NotifyStructurePlaceableVisibleState(bool state) 
    {
        s_allowedToZoomWithScroll = !state;
	}

    private void UpdateZoom()
    {
        _cameraZoomChild.transform.localPosition = _zoomAmount * -_zoomMagnitude * Camera.main.transform.forward;
    }
    private void SetZoomAmount(float amount)
    {
        _targetZoomAmount = amount;
        _zoomAmount = amount;
        UpdateZoom();
    }

    private void SetZoomForExplosivePath()
    {
        _targetZoomAmount = _startZoom;
    }

    public void ZoomCamera(InputAction.CallbackContext context)
    {
        if (!s_allowedToZoomWithScroll && !s_OverrideZoomAllow)
            return;
        if (_isCameraInCinematic)
            return;
        _zoomAxis = context.ReadValue<Vector2>().y;
        _targetZoomAmount -= _zoomAxis * Time.deltaTime * _zoomSpeed;
        _targetZoomAmount = Mathf.Clamp(_targetZoomAmount,_minZoom, _maxZoom);
    }

    public void OverrideZoomAllow(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
			s_OverrideZoomAllow = true;
		}
        else if (context.canceled) 
        {
			s_OverrideZoomAllow  = false;
		}
    }

    private IEnumerator CinematicLerpToZoomAmount(float targetZoom, Action doneWithZoom = null)
    {
        _cinematicStartLerpZoomAmount = _zoomAmount;
        _cinematicZoomLerpTimeElapsed = 0;
        while (_cinematicZoomLerpTimeElapsed < _cinematicZoomLerpDuration)
        {
            float t = _cinematicZoomLerpTimeElapsed / _cinematicZoomLerpDuration;
            t = Mathf.Sin(t * Mathf.PI * _cinematicZoomLerpSmoothing);
            _zoomAmount = Mathf.Lerp(_cinematicStartLerpZoomAmount, targetZoom, t);
            _cinematicZoomLerpTimeElapsed += Time.deltaTime;
            UpdateZoom();
            yield return null;
        }
        doneWithZoom?.Invoke();
	}

    private void LateUpdate()
    {
        if (_isCameraInCinematic)
            return;

        _zoomAmount = Mathf.Lerp(_zoomAmount, _targetZoomAmount, _zoomLerpSmoothing * Time.deltaTime);
        UpdateZoom();
    }
    #endregion
}
