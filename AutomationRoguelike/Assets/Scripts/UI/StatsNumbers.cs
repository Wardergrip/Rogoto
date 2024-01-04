using TMPro;
using UnityEngine;

public class StatsNumbers : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _icon;
	[SerializeField] private TextMeshProUGUI _statText;
	[SerializeField] private TextMeshProUGUI _upgradeText;
	[SerializeField] private RectTransform _border;
	[SerializeField] private RectTransform _transform;
	[SerializeField] private RectTransform _arrow;

	public void InitializeStat(Stat stat, int statAmount, int order)
	{
		UpdateStat(stat);
		Vector3 scale = new(_border.localScale.x, (_border.localScale.y * 2) /statAmount, _border.localScale.z);
		_border.localScale = scale;
	}

	public void UpdateStat(Stat stat)
	{
		_icon.text = $"<sprite name={stat.GetName()}>";
		float value = stat.FinalValue;
		_statText.text = value.ToString("F2").TrimEnd('0').TrimEnd('.').TrimEnd(',');
		if (stat.CurrentTier < 3 && stat.FinalValue != stat.PredictedNextValue)
		{
			value = stat.PredictedNextValue;
			_upgradeText.text = value.ToString("F2").TrimEnd('0').TrimEnd('.').TrimEnd(',');
			_arrow.gameObject.SetActive(true);
		}
		else
		{
			_upgradeText.text = "";
			_arrow.gameObject.SetActive(false);
		}
	}
}
