using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class HintShower : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private TextMeshProUGUI _tmpro;
	[SerializeField] private RectTransform _background;
	[SerializeField] private Vector2 _padding;

	private void Awake()
	{
		Hide();
		SetCaveSelectedText();
	}

	private void Start()
	{
		GameSystem.OnBuildPhaseStarted += SetBuildPhaseText;
		GameSystem.OnNewCaveSelected += SetCaveSelectedText;
		GameSystem.OnWaveStarted += SetWaveStartedText;
		GameSystem.OnNewCaveRevealed += SetNewCaveRevealedText;
	}

	private void OnDestroy()
	{
		GameSystem.OnBuildPhaseStarted -= SetBuildPhaseText;
		GameSystem.OnNewCaveSelected -= SetCaveSelectedText;
		GameSystem.OnWaveStarted -= SetWaveStartedText;
		GameSystem.OnNewCaveRevealed -= SetNewCaveRevealedText;
	}

	private void SetBuildPhaseText()
	{
		StringBuilder sb = new();
		sb.AppendLine("Build miners on resources and");
		sb.AppendLine("link them to deposit boxes.");
		sb.AppendLine("Set up defences by building turrets");
		sb.AppendLine("near paths you mined out.");
		UpdateText(sb.ToString());
	}
	private void SetCaveSelectedText()
	{
		StringBuilder sb = new();
		sb.AppendLine("Mine out a path by placing explosions");
		sb.AppendLine("from the blue orbs to the red orbs.");
		sb.AppendLine("Right click to undo, scroll or F to rotate");
		UpdateText(sb.ToString());
	}

	private void SetWaveStartedText()
	{
		StringBuilder sb = new();
		sb.AppendLine("Enemies are attacking your base.");
		sb.AppendLine("Click to shoot a mortar.");
		UpdateText(sb.ToString());
	}

	private void SetNewCaveRevealedText()
	{
		StringBuilder sb = new();
		sb.AppendLine("Select a wave to mine towards.");
		sb.AppendLine("The nest level indicates it's strength.");
		sb.AppendLine("Pay attention to the shipwrecks");
		sb.AppendLine("and resources that will be unlocked");
		sb.AppendLine("once the nest is destroyed.");
		UpdateText(sb.ToString());
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Show();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		Hide();
	}

	public void UpdateText(string text)
	{
		_tmpro.SetText(text);
		_tmpro.ForceMeshUpdate(true);

		Vector2 sizeDelta = _tmpro.GetRenderedValues(false);
		_background.sizeDelta = sizeDelta + _padding;
	}

	public void Show()
	{
		_tmpro.gameObject.SetActive(true);
		_background.gameObject.SetActive(true);
	}

	public void Hide()
	{
		_tmpro.gameObject.SetActive(false);
		_background.gameObject.SetActive(false);
	}
}
