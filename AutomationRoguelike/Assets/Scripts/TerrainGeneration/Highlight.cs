using System.Collections.Generic;
using System.Resources;
using UnityEngine;
using UnityEngine.EventSystems;

public class Highlight : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private List<GameObject> _removeOnPick = new List<GameObject>();
	[SerializeField] private List<GameObject> _activeOnHighlight = new List<GameObject>();

	[SerializeField] private LineRenderer _lineRenderer;
	[SerializeField] private float _lineRendererHeight;

	private void Awake()
	{
		foreach (GameObject go in _activeOnHighlight)
		{
			go.SetActive(false);
		}
	}

	public void Pick()
	{
		foreach (GameObject go in _removeOnPick)
		{
			Destroy(go);
		}
		_removeOnPick.Clear();

		foreach (GameObject go in _activeOnHighlight)
		{
			go.SetActive(true);
		}
		Destroy(GetComponent<BoxCollider>());
	}

	public void Connect(Vector3[] positions)
	{
		List<Vector3> interpolatedPositions = new List<Vector3>();

		for (int i = 0; i < positions.Length - 1; i++)
		{
			interpolatedPositions.Add(positions[i]);

			Vector3 newPos = Vector3.Lerp(positions[i], positions[i + 1], 0.5f);
			interpolatedPositions.Add(newPos);
		}

		interpolatedPositions.Add(positions[positions.Length - 1]);

		for (int i = 0; i < interpolatedPositions.Count; i++)
		{
			Vector3 pos = interpolatedPositions[i];
			pos.y += _lineRendererHeight;
			interpolatedPositions[i] = pos;
		}

		_lineRenderer.positionCount = interpolatedPositions.Count;
		_lineRenderer.SetPositions(interpolatedPositions.ToArray());
	}


	public void OnPointerEnter(PointerEventData eventData)
	{
		foreach (GameObject go in _activeOnHighlight)
		{
			go.SetActive(true);
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		foreach (GameObject go in _activeOnHighlight)
		{
			if (go != null)
			{
				go.SetActive(false);
			}
		}
	}
}
