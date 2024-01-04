using System;
using UnityEngine;

public class GameSystem : MonoStatic<GameSystem>
{
	private int _waveCounter = 1;
	private int _luck;
	public int WaveCounter { get => _waveCounter; }
	public int Luck { get => _luck; set => _luck = value; }
	private float _gameSpeed = 1.0f;
	public float GameSpeed 
	{
		get => _gameSpeed;
		set
		{
			if (value == 0.0f)
			{
				PauseGame();
			}
			else if (IsPaused)
			{
				UnpauseGame();
			}
			_gameSpeed = Mathf.Max(value, 0.0f);
		}
	}
	public bool IsPaused { get; private set; }

	public static bool s_AllowStartWave { get; private set; }
	public static bool s_AllowExplode { get; set; }

	public static event Action OnBuildPhaseStarted ;
	public static event Action OnWaveStarted ;
	public static event Action OnEnemyStartSpawning ;
	public static event Action OnEnemyAllKilled ;
	public static event Action OnRewardClaimed ;
	public static event Action OnNewCaveRevealed ;
	public static event Action OnNewCaveSelected ;
	public static event Action OnPauseGame ;
	public static event Action OnUnpauseGame ;
	public static event Action OnShowGameOverScreen;
	public static event Action OnNestDamaged;

	protected override void LateAwake()
	{
		OnBuildPhaseStarted += AllowWaveStart;
		OnWaveStarted += DisallowWaveStart;
		OnNewCaveSelected += AllowExplode;
		OnBuildPhaseStarted += DisallowExplode;
	}

	private void OnDestroy()
	{
		// On app quit, instance will be null, see monostatic
		if (Instance == null) return;
		OnBuildPhaseStarted -= AllowWaveStart;
		OnWaveStarted -= DisallowWaveStart;
		OnNewCaveSelected -= AllowExplode;
		OnBuildPhaseStarted -= DisallowExplode;
	}

	public void PauseGame()
	{
		if (IsPaused)
		{
			return;
		}
		IsPaused = true;
		_gameSpeed = 0.0f;
		SetActiveAllNavMeshAgents(false);
		OnPauseGame?.Invoke();
	}

	public void UnpauseGame()
	{
		if (!IsPaused)
		{
			return;
		}
		IsPaused = false;
		_gameSpeed = 1.0f;
		SetActiveAllNavMeshAgents(true);
		OnUnpauseGame?.Invoke();
	}

	public void StartWave()
	{
		CameraController cameraController = FindObjectOfType<CameraController>();
		if (s_AllowStartWave && (cameraController != null && !cameraController.IsCameraInCinematic))
			OnWaveStarted?.Invoke();
	}
	public void RewardClaimed()
	{
		++_waveCounter;
		OnRewardClaimed.Invoke();
	}
	public void CavesSpawned()
	{
		OnNewCaveRevealed.Invoke();
	}
	public void CaveChosen()
	{
		OnNewCaveSelected.Invoke();
	}
	public void PathExploded()
	{
		OnBuildPhaseStarted.Invoke();
	}
	public void NestDamaged()
	{
		OnNestDamaged?.Invoke();
	}
	public void ShowGameOverScreen()
	{
		OnShowGameOverScreen?.Invoke();
	}
	public void StartEnemySpawning()
	{
		if (EnemyNest.ActiveNests.Count > 0)
		{
			OnEnemyStartSpawning.Invoke();
		}
		else
		{
			OnRewardClaimed.Invoke();
		}
	}
	public void CheckEnemiesRemaining()
	{
		if (Enemy.LiveEnemies.Count > 0)
			return;
		foreach (EnemyNest nest in EnemyNest.ActiveNests)
		{
			if (nest.LoadedSwarms.Count > 0)
				return;
		}
		OnEnemyAllKilled.Invoke();
	}

	private void SetActiveAllNavMeshAgents(bool state)
	{
		Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
		foreach (Enemy enemy in enemies)
		{
			enemy.UpdateMovementSpeed();
		}
	}

	private void AllowWaveStart()
	{
		s_AllowStartWave = true;
	}
	private void DisallowWaveStart()
	{
		s_AllowStartWave = false;
	}
	private void AllowExplode()
	{
		s_AllowExplode = true;
	}
	private void DisallowExplode()
	{
		s_AllowExplode = false;
	}
}
