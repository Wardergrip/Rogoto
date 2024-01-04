using UnityEngine;
using UnityEngine.Events;

public class CanvasManager : MonoBehaviour
{
	[SerializeField] private Animator _buildPhaseHUD;
	[SerializeField] private Animator _explodePathPhaseHUD;
	[SerializeField] private Animator _combatHUD;
	[SerializeField] private Animator _effectRewardHUD;
	[SerializeField] private Animator _caveSeclectorHUD;
	 private Animator _activeHud;

	[Header("Events")]
	public UnityEvent OnShowBuildPhaseHUD;
	public UnityEvent OnShowExplodePhaseHUD;
	public UnityEvent OnShowCombatHUD;
	public UnityEvent OnShowEffectRewardHUD;
	public UnityEvent OnShowCaveSelectorHUD;

	private void Awake()
	{
		GameSystem.OnBuildPhaseStarted += SwitchToBuildPhaseHUD;
		GameSystem.OnNewCaveSelected += SwitchToExplodePathPhaseHUD;
		GameSystem.OnWaveStarted += SwitchToCombatHUD;
		GameSystem.OnNestDamaged += SwitchToEffectRewardHUD;
		GameSystem.OnRewardClaimed += SwitchToCaveSelectorHUD;

		_buildPhaseHUD.gameObject.SetActive(false);
		_explodePathPhaseHUD.gameObject.SetActive(false);
		_combatHUD.gameObject.SetActive(false);
		_effectRewardHUD.gameObject.SetActive(false);
		_caveSeclectorHUD.gameObject.SetActive(false);
	}

	private void OnDestroy()
	{
		GameSystem.OnBuildPhaseStarted -= SwitchToBuildPhaseHUD;
		GameSystem.OnNewCaveSelected -= SwitchToExplodePathPhaseHUD;
		GameSystem.OnWaveStarted -= SwitchToCombatHUD;
		GameSystem.OnNestDamaged -= SwitchToEffectRewardHUD;
		GameSystem.OnRewardClaimed -= SwitchToCaveSelectorHUD;
	}
	public void HideHud()
	{
        _activeHud.SetTrigger("Exit");  
    }
    public void SHowHud()
	{
		_activeHud.gameObject.SetActive(true);
        _activeHud.SetTrigger("Entry");
    }

	private void SwitchToHUD(Animator hud)
	{
		if (_activeHud != null)
		_activeHud.SetTrigger("Exit");
		hud.gameObject.SetActive(true);
		hud.SetTrigger("Entry");
		_activeHud = hud;
	}

	private void SwitchToBuildPhaseHUD() 
	{
		SwitchToHUD(_buildPhaseHUD);
		OnShowBuildPhaseHUD?.Invoke();
	}
	private void SwitchToExplodePathPhaseHUD()
	{
		SwitchToHUD(_explodePathPhaseHUD);
		OnShowExplodePhaseHUD?.Invoke();
	}
	private void SwitchToCombatHUD()
	{
		SwitchToHUD(_combatHUD);
		OnShowCombatHUD?.Invoke();
	}
	private void SwitchToEffectRewardHUD()
	{
		SwitchToHUD(_effectRewardHUD);
		OnShowEffectRewardHUD?.Invoke();
	}
	private void SwitchToCaveSelectorHUD()
	{
		SwitchToHUD(_caveSeclectorHUD);
		OnShowCaveSelectorHUD?.Invoke();
	}
}
