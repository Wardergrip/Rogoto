using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class GuideText : MonoStatic<GuideText>
{
	[SerializeField] private TextMeshProUGUI _text;
	[SerializeField] private GameObject _backGround;

	private void Start()
	{
		GameSystem.OnNewCaveSelected += SetCaveSelectedText;
		GameSystem.OnNewCaveRevealed += SetNewCaveRevealedText;
		GameSystem.OnBuildPhaseStarted += Hide;
	}

	private void OnDestroy()
	{
		GameSystem.OnNewCaveSelected -= SetCaveSelectedText;
		GameSystem.OnNewCaveRevealed -= SetNewCaveRevealedText;
		GameSystem.OnBuildPhaseStarted -= Hide;
	}

	private void SetActive(bool state)
	{
		_text.gameObject.SetActive(state);
		if (_backGround) _backGround.SetActive(state);
	}

	private void Hide() => SetActive(false);
	private void Show() => SetActive(true);

	private void SetCaveSelectedText()
	{
		Show();
		UpdateText("Mine a path towards the cave");
	}

	private void SetNewCaveRevealedText()
	{
		Show();
		UpdateText("Select a cave to mine towards");
	}

	public void UpdateText(string text)
	{
		_text.text = text;
	}
}
