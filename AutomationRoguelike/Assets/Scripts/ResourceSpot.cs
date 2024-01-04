using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ResourceSpot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private GameObject _lockedObject;
	[SerializeField] private GameObject _canvasObject;
	[SerializeField] private GameObject _fullObject;
	[SerializeField] private GameObject _minedObject;
	[SerializeField] private GameObject _highlightObject;
	[SerializeField] private TextMeshProUGUI _text;
	[SerializeField] private int _level = 0;
	[SerializeField] private bool _locked = true;
	[SerializeField] private bool _occupied = false;

	public static List<ResourceSpot> s_ResourceSpots = new();

	public int Level
	{
		get => _level; set => _level = value;
	}

	public void Initialize(int level)
	{
		_level = level;
		_text.SetText(level.ToString());
	}

	private void Awake()
	{
		s_ResourceSpots.Add(this);
		GameSystem.OnRewardClaimed += Unlock;
	}

	private void OnDestroy()
	{
		s_ResourceSpots.Remove(this);
		GameSystem.OnRewardClaimed -= Unlock;
	}

	public void Unlock()
	{
		if ((GameSystem.Instance.WaveCounter - 1) % 2 == 0)
		{
			_locked = false;
			_lockedObject.SetActive(false);
			GameSystem.OnRewardClaimed -= Unlock;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		foreach (ResourceSpot rs in s_ResourceSpots)
		{
			rs.showCanvas(true);
		}
	}
	public void OnPointerExit(PointerEventData eventData)
	{
		foreach (ResourceSpot rs in s_ResourceSpots)
		{
			rs.showCanvas(false);
		}
	}

	public void showCanvas(bool show)
	{
		_canvasObject.SetActive(show);
	}

	public void Occupied(bool occupied)
	{
		if (occupied)
		{
			_fullObject.SetActive(false);
			_minedObject.SetActive(true);
		}
		else
		{
			_fullObject.SetActive(true);
			_minedObject.SetActive(false);
		}
		_occupied = occupied;
	}
	public bool IsOccupied()
	{
		return _occupied;
	}

	public void ChangeValueUnoccupied(int value)
	{
		if (!_occupied)
		{
			_level += value;
			_text.SetText(_level.ToString());
		}
	}

	public void HighlightResource(bool highlight)
	{
		_highlightObject.SetActive(highlight);
	}
}
