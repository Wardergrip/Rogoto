using System.Collections;
using UnityEngine;

public class TerrainGenerator : MonoStatic<TerrainGenerator>
{
	[SerializeField] private LevelGenerator _levelGenerator;
	[SerializeField] private ChunkPicker _chunkPicker;

	private void Start()
	{
		GameSystem.OnRewardClaimed += SpawnCaves;
		GameSystem.OnBuildPhaseStarted += CavesExploded;

		StartCoroutine(SpawnChunks());
	}

	private IEnumerator SpawnChunks()
	{
		yield return null;
		Random.InitState((int)System.DateTime.Now.Ticks);
		_levelGenerator.SpawnCaves();
		_levelGenerator.Submit(0);
	}

	private void OnDestroy()
    {
        GameSystem.OnRewardClaimed -= SpawnCaves;
		GameSystem.OnBuildPhaseStarted -= CavesExploded;
    }

    private void SpawnCaves()
	{
		if ((GameSystem.Instance.WaveCounter - 1) % 2 == 0)
		{
			_levelGenerator.SpawnCaves();
			_chunkPicker.SetCanChoose(true);
			GameSystem.Instance.CavesSpawned();
		}
		else
		{
			GameSystem.Instance.PathExploded();
		}
	}

	private void CavesExploded()
	{
		_levelGenerator.CavesExploded();
	}

	public void CaveChosen()
	{ 
		GameSystem.Instance.CaveChosen();
	}
}
