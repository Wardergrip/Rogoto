using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum PlacementWeight
{
    Light, Normal, Heavy
}
public class StructurePlacer : MonoBehaviour
{
	private Blueprint _blueprint;
	private GameObject _structureModel;
	private bool _validPlacement = true;
	private bool _allowedToPlaceByOtherScripts = false;
	[SerializeField] private TextMeshProUGUI _costCounter;
	private Renderer[] _renderers;
	[SerializeField] private GameObject _turretRangePreFab;
	[SerializeField] private Material _validPlacementMaterial;
	[SerializeField] private Material _inValidPlacementMaterial;
	public UnityEvent OnInvalidPlacement;
	public UnityEvent OnStructurePlacementLight;
	public UnityEvent OnStructurePlacementNormal;
	public UnityEvent OnStructurePlacementHeavy;
	public UnityEvent OnNotEnoughGold;
	private bool _placeButtonPressed;
	private readonly float _previewSizeIncrease = 1.05f;
	private Vector3 _oldPosition;
	[SerializeField] private LayerMask _mask;
	private bool _newPosSet = false;
	private int _layerClamp = 0;
	private bool _isMiner = false;
	private bool _onGround = true;
	private bool _onResource = true;
	private List<TurretRange> _turretRanges = new();
	private bool _isTurret = false;
	private List<InputOutputVisual> _inputsOutputs = new();
	public bool AllowedToPlaceByOtherScripts { get => _allowedToPlaceByOtherScripts; set => _allowedToPlaceByOtherScripts = value; }
	private static float s_rotation = 0f;
	private List<Collider> _colliders = new();

	public void SetUp(Blueprint blueprint)
	{
		_isTurret = false;
		_isMiner = false;
		_blueprint = blueprint;
		PlayerStructure structure = _blueprint.Structure;
		UpdateCostCounter();
		List<Texture2D> imgs = null;
		if (structure.GetComponent<Turret>() != null)
		{
			_isTurret = true;
			imgs = GetFinalTurretRange(structure);
			_layerClamp = 1;
		}
		else
		{
			if ((structure.GetComponent<Miner>() != null))
			{
				_isMiner = true;
			}
		}
		foreach (ResourceSpot rs in ResourceSpot.s_ResourceSpots)
		{
			rs.HighlightResource(_isMiner);
		}

		_structureModel = Instantiate(structure.Model, transform);
		if (imgs != null)
		{
			SpawnTurretRangePreview(imgs);
		}

		_structureModel.transform.localScale = new Vector3(_previewSizeIncrease, _previewSizeIncrease, _previewSizeIncrease);
		_renderers = _structureModel.transform.GetComponentsInChildren<Renderer>();

		if (!_isTurret)
		{
			SpawnInputOutputPreview(structure.GetComponent<Machine>());
		}

		SetValidPlacementMaterial();
		foreach (BoxCollider col in structure.PlacementColliders) // Casting
		{
			BoxCollider copy = gameObject.AddComponent<BoxCollider>();
			copy.center = col.center;
			copy.size = col.size;
			copy.isTrigger = true;
			_colliders.Add(copy);
		}
		transform.Rotate(transform.up, s_rotation);
	}
	private void UpdateCostCounter()
	{
        _costCounter.text = "-" + _blueprint.Structure.Cost;
    }

	private void SpawnInputOutputPreview(Machine machine)
	{
		foreach (var input in machine.Inputs)
		{
			GameObject newInputObject = Instantiate(input.gameObject, transform, _structureModel.transform);
			Component[] components = newInputObject.GetComponentsInChildren<Component>();
			foreach (var component in components)
			{
				// Check if the component is not a MeshRenderer or MeshFilter
				if (!(component is MeshRenderer) && !(component is MeshFilter) && !(component is Transform) && !(component is InputOutputVisual))
				{
					Destroy(component);
				}
			}
			InputOutputVisual io = newInputObject.GetComponent<InputOutputVisual>();
			_inputsOutputs.Add(io);
		}
		foreach (var output in machine.Outputs)
		{
			GameObject newOutputObject = Instantiate(output.gameObject, transform, _structureModel.transform);
			Component[] components = newOutputObject.GetComponentsInChildren<Component>();
			foreach (var component in components)
			{
				// Check if the component is not a MeshRenderer or MeshFilter
				if (!(component is MeshRenderer) && !(component is MeshFilter) && !(component is Transform) && !(component is InputOutputVisual))
				{
					Destroy(component);
				}
			}
			InputOutputVisual io = newOutputObject.GetComponent<InputOutputVisual>();
			_inputsOutputs.Add(io);
		}
	}

	private void UpdateInputOutputVisuals(bool validPosition)
	{
		if (_isTurret)
			return;
		if (_isMiner && !_onResource)
			validPosition = false;
		foreach (InputOutputVisual io in _inputsOutputs)
		{
			io.UpdateVisual(validPosition, false);
		}
	}

	#region Turret range preview
	private List<Texture2D> GetFinalTurretRange(PlayerStructure structure)
	{
		List<Texture2D> turretTextures = new List<Texture2D>();
		structure.gameObject.SetActive(false);
		PlayerStructure struc = Instantiate(structure, transform);
		Turret[] turrets = struc.GetComponentsInChildren<Turret>();
		foreach (Turret turret in turrets)
		{
			if (turret.SpawnObjectsFromImage != null)
			{
				Texture2D img = turret.SpawnObjectsFromImage.ImageTexture;
				turretTextures.Add(img);
			}
		}
		structure.gameObject.SetActive(true);
		Destroy(struc.gameObject);
		return turretTextures;
	}
	private void SpawnTurretRangePreview(List<Texture2D> imgs)
	{
		foreach (Texture2D img in imgs)
		{
			_structureModel.SetActive(false);
			SpawnObjectsFromImage spawnObjectsFromImage = _structureModel.AddComponent<SpawnObjectsFromImage>();
			spawnObjectsFromImage.ImageTexture = img;
			spawnObjectsFromImage.Height = 0;
			spawnObjectsFromImage.ObjectToSpawn = _turretRangePreFab;
			spawnObjectsFromImage.OnObjectsSpawned.AddListener(() => { CollectTurretRanges(spawnObjectsFromImage); });
			_structureModel.SetActive(true);
		}
	}
	private void CollectTurretRanges(SpawnObjectsFromImage imgs)
	{
		foreach (GameObject go in imgs.SpawnedObjects)
		{
			_turretRanges.Add(go.GetComponent<TurretRange>());
		}
	}
	#endregion

	private void FixedUpdate()
	{
		SetPosition();
	}
	private void LateUpdate()
	{
		if (_newPosSet)
		{
			AllChecksGround();
			CheckPlacement();
			_newPosSet = false;
		}
	}

	private void Start()
	{
		CameraController.NotifyStructurePlaceableVisibleState(true);
	}

	public void Remove()
	{
		CameraController.NotifyStructurePlaceableVisibleState(false);
	}
	private void CheckGround()
	{
		RaycastHit hit;
		bool onGround = true;
		foreach (Collider collider in _colliders)
		{
            if (!Physics.Raycast(collider.bounds.center , Vector3.down, out hit, 0.6f, _mask))
            {
				onGround = false;
				break;
            }
        }
		if (onGround)
		{
			_onGround = true;
			return;
		}
		else
		{
			_onGround = false;
			return;
		}
	}
	private ResourceSpot CheckResourceTile()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 0.6f, _mask))
		{
			ResourceSpot rs = hit.collider.GetComponent<ResourceSpot>();
			if (rs != null)
			{
				_onResource = true;
				return rs;
			}
		}
		_onResource = false;
		return null;
	}
	private void CheckPlacement()
	{
		if (_placeButtonPressed == false || !AllowedToPlaceByOtherScripts)
			return;
		if (!_validPlacement || !_onGround)
		{
			OnInvalidPlacement.Invoke();
			return;
		}
		if (_isMiner && !_onResource)
		{
			OnInvalidPlacement.Invoke();
			return;
		}
		int cost = _blueprint.Structure.Cost;
		if (cost > EconomyManager.Instance.Gold)
		{
			OnNotEnoughGold.Invoke();
			return;
		}
		switch (_blueprint.Structure.VisualWeight)
		{
			case PlacementWeight.Light:
				OnStructurePlacementLight.Invoke();
				break;
			case PlacementWeight.Normal:
				OnStructurePlacementNormal.Invoke();
				break;
			case PlacementWeight.Heavy:
				OnStructurePlacementHeavy.Invoke();
				break;
		}

		PlaceStructure(cost);
	}

	private void PlaceStructure(int cost)
	{
		PlayerStructure placed = Instantiate(_blueprint.Structure, transform.position, transform.rotation);

		EconomyManager.Instance.RemoveGold(cost);
		_blueprint.Variant?.Setup(placed);
		Physics.Simulate(Time.deltaTime);
		placed.OriginBlueprint = _blueprint;
		_blueprint.Structure.Cost += _blueprint.Structure.CostIncrease;
		UpdateCostCounter();
	}

	public void Place(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			_placeButtonPressed = true;
			CheckPlacement();
		}
		else if (context.canceled)
		{
			_placeButtonPressed = false;
		}
	}
	private void AllChecksGround()
	{
		bool ground = _onGround;
		bool resource = _onResource;
		CheckGround();
		CheckResourceTile();
		if (_onGround != ground)
		{
			if (!_onGround)
				SetInValidPlacementMaterial();
			else if (_validPlacement)
				SetValidPlacementMaterial();
		}
		if (_isMiner && _onResource != resource)
		{
			if (!_onResource)
				SetInValidPlacementMaterial();
			else if (_onResource && _validPlacement)
				SetValidPlacementMaterial();
		}
		UpdateTurretRangeVisual(_validPlacement);
		if (!_onGround)
			UpdateInputOutputVisuals(false);
		else
		{
			UpdateInputOutputVisuals(_validPlacement);
		}
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

			if (scrollAxis < 0)
			{
				transform.Rotate(transform.up, -90);
				s_rotation -= 90;
			}
			else if (scrollAxis > 0)
			{
				transform.Rotate(transform.up, 90);
				s_rotation += 90;
			}

			//UpdateTurretRangeVisual(_validPlacement);
			//if (!_onGround)
			//	UpdateInputOutputVisuals(false);
			//else
			//{
			//	UpdateInputOutputVisuals(_validPlacement);
			//}
			StartCoroutine(RotateUpdate());
		}
	}
	private IEnumerator RotateUpdate()
	{
		yield return null;
		yield return null;
        _newPosSet = true;
    }

	public void RotateKeyboard(InputAction.CallbackContext context)
	{
		if (context.performed)
		{
			transform.Rotate(transform.up, 90);
			s_rotation += 90;

			UpdateTurretRangeVisual(_validPlacement);
			if (!_onGround)
				UpdateInputOutputVisuals(false);
			else
			{
				UpdateInputOutputVisuals(_validPlacement);
			}
            StartCoroutine(RotateUpdate());
        }
	}
	private void SetValidPlacementMaterial()
	{
		foreach (Renderer renderer in _renderers)
		{
			renderer.material = _validPlacementMaterial;
		}
	}
	private void SetInValidPlacementMaterial()
	{
		foreach (Renderer renderer in _renderers)
		{
			renderer.material = _inValidPlacementMaterial;
		}
	}
	private void UpdateTurretRangeVisual(bool validPosition)
	{
		if (!_isTurret)
			return;
		foreach (TurretRange turrerRange in _turretRanges)
		{
			turrerRange.UpdateVisual(validPosition, _onGround);
		}
	}
	private void SetPosition()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100, _mask))
		{
			transform.position = SnapToGrid(hit.point);
			if (transform.position != _oldPosition)
				NewPositionSet();
			_oldPosition = transform.position;
		}
	}
	private void NewPositionSet()
	{
		_newPosSet = true;
	}

	//clamps it 0 for the time being
	private Vector3 SnapToGrid(Vector3 position)
	{
		Vector3 snapped = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
		Vector3 clamped = new Vector3(snapped.x, Mathf.Clamp(snapped.y, _layerClamp, _layerClamp), snapped.z);
		return clamped;
	}
	private void OnTriggerStay(Collider other)
	{
		if (_validPlacement == true)
			SetInValidPlacementMaterial();
		_validPlacement = false;
	}
	private void OnTriggerExit(Collider other)
	{
		if (_validPlacement == false && _onGround)
		{
			SetValidPlacementMaterial();
			if (_isMiner && !_onResource)
				SetInValidPlacementMaterial();
		}
		_validPlacement = true;
	}
}
