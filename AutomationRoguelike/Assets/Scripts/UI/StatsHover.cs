using TMPro;
using UnityEngine;

public class StatsHover : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _icon;
	[SerializeField] private TextMeshProUGUI _statText;
	[SerializeField] private RectTransform _border;
	[SerializeField] private RectTransform _overlay;
	[SerializeField] private RectTransform _iconSocket;
	[SerializeField] private RectTransform _statSocket;

	public void InitializeStat(Stat stat, int statAmount)
	{
		UpdateStat(stat);
		Vector3 scale = new((_border.localScale.x * 2) / statAmount, _border.localScale.y, _border.localScale.z);
		_border.localScale = scale;
		_overlay.localScale = scale;

		_icon.gameObject.transform.position = _iconSocket.position;
		_statText.gameObject.transform.position = _statSocket.position;
	}

	public void UpdateStat(Stat stat)
	{
		_icon.text = $"<sprite name={stat.GetName()}>";
		float value = stat.FinalValue;
		_statText.text = value.ToString("F4").TrimEnd('0').TrimEnd('.').TrimEnd(',');
	}
}