using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class LoadingText : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _tmpro;
	[SerializeField] private float _updateInterval = 0.5f;
	private string _text;

	private void Awake()
	{
		_text = _tmpro.text;
		StartCoroutine(UpdateText());
	}


	private IEnumerator UpdateText()
	{
		int n = 0;
		StringBuilder sb = new();
		while (true)
		{
			yield return new WaitForSeconds(_updateInterval);
			for (int i = 0; i < n; ++i)
			{
				sb.Append(".");
			}
			_tmpro.text = $"{_text}{sb}";
			n = (n + 1) % 4;
			sb.Clear();
		}
	}
}
