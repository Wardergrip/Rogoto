using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Trivial;
using System.Collections;

public class Explosive : MonoBehaviour
{
	[SerializeField] private LayerMask _miningAreaMask;
	[SerializeField] private LayerMask _setPositionMask;
	[SerializeField] private Sprite _previewIcon;

	public Sprite PreviewIcon { get => _previewIcon;  }
	public bool IsConnectedToEnd { get => _isConnectedToEnd; }
	public static List<Explosive> s_ActiveExplosives { get => s_activeExplosives; set => s_activeExplosives = value; }

	private bool _isValidConnected;
	private bool _isValidPosition=true;
	private bool _isConnectedToEnd;
	private bool _isPlaced;
	private static List<Explosive> s_activeExplosives = new();
	[SerializeField] private Transform[] _oneTileChildren;
	[SerializeField] private Material _validMat;
	[SerializeField] private Material _invalidMat;
	[SerializeField] private Material _bombMat;
	private Renderer[] _childRenderers;
	[SerializeField] UnityEvent _onInvalidPlacement;
	private List<Transform> _validConnectionsOnThis;
	[SerializeField] private LayerMask _airCheckMask;

	public UnityEvent<Explosive> OnMove;
	public UnityEvent<Transform> OnExplode;
	private Vector3 _previousPosition;

	private Coroutine _explodeCoroutine;

	private void Start()
	{
		_previousPosition = transform.position;
		_validConnectionsOnThis = transform.AllChildren().Where(x => x.transform.CompareTag(("ExplosiveValidConnectionCollider"))).ToList();
		_validConnectionsOnThis.SetActiveAll(false);
		_childRenderers = GetComponentsInChildren<Renderer>().Where(x => !x.transform.parent.CompareTag("ExplosiveValidConnectionCollider")).ToArray();
	}
	public void Explode(float waitTimePerShape)
	{
		if (_explodeCoroutine != null)
		{
			return;
		}
		_explodeCoroutine = StartCoroutine(ExplodeCoroutine(waitTimePerShape));
	}
	public void SetVisualVisibility(bool state)
	{
		foreach (Transform child in _oneTileChildren)
		{
			MeshRenderer meshRenderer = child.GetComponentInChildren<MeshRenderer>();
			if (meshRenderer != null)
			{
				meshRenderer.enabled = state;
			}
		}
	}
	private IEnumerator ExplodeCoroutine(float waitTimePerShape)
	{
		List<Transform> oneTileChildren = _oneTileChildren.ToList().SortByDistance(Base.Position);
		float waitTimePerTile = waitTimePerShape / oneTileChildren.Count;
		foreach (Transform child in oneTileChildren)
		{
			OnExplode.Invoke(child);

			Vector3 offset = new(child.position.x, child.position.y - 0.5f, child.position.z);

			const float checkSize = 0.2f;
			Collider[] colliders = Physics.OverlapSphere(offset, checkSize);

			foreach (Collider collider in colliders)
			{
				if (collider.transform.parent != null && collider.transform.parent.gameObject.GetComponent<Tile>())
				{
					Destroy(collider.gameObject);
				}
			}

			yield return new WaitForSeconds(waitTimePerTile);
		}
		_explodeCoroutine = null;
		Destroy(gameObject);
	}

	public void Rotate(float value)
	{
		transform.Rotate(
			transform.up, value > 0 ? //Is bigger than 0? 
			90 : -90 // Yes / No
			);
	}
	private void SetMaterial()
	{
		foreach (Renderer renderer in _childRenderers)
		{
			renderer.material = _isPlaced ? // Is placed?
				_bombMat : ( // Yes
				_isValidPosition && _isValidConnected && !IsAirBelow() && IsMiningAreaBelow() ? //If not placed is it connected and is there no air below it?
				_validMat : _invalidMat // Yes / no
				);
		}
	}

	public bool TryPlace()
	{
		if (!_isValidConnected || !_isValidPosition)
		{
			_onInvalidPlacement.Invoke();
			return false;
		}

		if (IsAirBelow())
		{
			_onInvalidPlacement.Invoke();
			return false;
		}

		if (!IsMiningAreaBelow())
		{
			_onInvalidPlacement.Invoke();
			return false;
		}

		_isPlaced = true;
		s_ActiveExplosives.Add(this);
		SetMaterial();
		_validConnectionsOnThis.SetActiveAll(true);
		return true;
	}

	private bool IsAirBelow()
	{
		bool airGap = false;

		foreach (Transform child in _oneTileChildren)
		{
			RaycastHit hit;
			if (!(Physics.Raycast(child.position, Vector3.down, out hit, 1f, _airCheckMask) && hit.collider != null))
			{
				airGap = true;
			}
		}

		return airGap;
	}

	private bool IsMiningAreaBelow()
	{
		bool area = false;

		foreach (Transform child in _oneTileChildren)
		{
			RaycastHit hit;
			if ((Physics.Raycast(child.position, Vector3.down, out hit, 5f, _miningAreaMask) && hit.collider != null))
			{
				area = true;
			}
		}

		return area;
	}

	private void OnTriggerStay(Collider other)
	{
		HandleTrigger(other, true);
	}
	private void OnTriggerExit(Collider other)
	{
		HandleTrigger(other, false);
	}
	private void HandleTrigger(Collider other, bool stay)
	{
		if (_isPlaced) 
			return;

		switch (other.tag)
		{
			case "ExplosiveValidConnectionCollider":
				_isValidConnected = stay;
				break;
			case "ExplosiveEndConnectionCollider":
				_isConnectedToEnd = stay;
				break;
			default:
				_isValidPosition = !stay;
				break;
		}
	}
	private void OnDestroy()
	{
		s_activeExplosives.Remove(this);
	}
	private void FixedUpdate()
	{
		if (_isPlaced)
			return;
		SetPosition();
		SetMaterial();
	}
	private void SetPosition()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(ray, out hit, 100,_setPositionMask))
		{
			transform.position = SnapToGrid(hit.point);
			if (transform.position != _previousPosition)
			{
				_previousPosition = transform.position;
				OnMove?.Invoke(this);
			}
		}
	}
	private Vector3 SnapToGrid(Vector3 position)
	{
		Vector3 snapped = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
		Vector3 clamped = new Vector3(snapped.x, Mathf.Clamp(snapped.y, 1.5f, 1.5f), snapped.z);
		return clamped;
	}
}
