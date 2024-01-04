using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.Events;

public class Chunk
{
	public Bounds Bounds { get; set; }
	public int ChunkWidth { get; set; }
	public int ChunkHeight { get; set; }
	public int BorderSize { get; set; }
	public bool IsLastChunk { get; set; }
	public ChunkGenerator Generator { get; set; }

	public Chunk(Bounds bounds, int chunkwidth, int chunkheight, int bordersize)
	{
		Bounds = bounds;
		ChunkWidth = chunkwidth;
		ChunkHeight = chunkheight;
		BorderSize = bordersize;
	}
	public Chunk()
	{
	}
	public void Merge()
	{
		Generator?.Solidify();
	}
	public void Detach()
	{
		Generator?.Detach();
	}
	public void SetLastChunk(bool b)
	{
		IsLastChunk = b;
	}
}
public class LevelGenerator : MonoBehaviour
{
	private class Connection
	{
		public Chunk chunk1;
		public Chunk chunk2;

		public Connection(Chunk chunk1, Chunk chunk2)
		{
			this.chunk1 = chunk1;
			this.chunk2 = chunk2;
		}
	}
	private enum Choice : int
	{
		Invalid = -1,
		Valid = 0,
		Potential = 1
	}


	[Header("Chunk Settings")]
	[SerializeField] private GameObject _chunkGenerator;
	[SerializeField] private GameObject _miningArea;
	[SerializeField] private int _minChunkSize = 16;
	[SerializeField] private int _maxChunkSize = 32;
	[SerializeField] private int _border = 5;

	[Header("ChunkGrowth")]
	[SerializeField] private int _minSizeGrowth = 4;
	[SerializeField] private int _maxSizeGrowth = 4;
	private int _amountPlaced = 0;

	[Header("Start Chunk")]

	[SerializeField] private GameObject _startingChunk;

	[Header("Generation")]
	[SerializeField] private int _branchLength = 5;
	[SerializeField] private int _minTouchingArea = 4;
	[SerializeField] private GameObject _miningAreaObject;
	private List<GameObject> _placedMiningBounds = new();
	[SerializeField] private GameObject _highlightObject;
	private List<GameObject> _highlightObjects = new();
	//[SerializeField] private int _miningAreaSize = 24;
	private readonly List<Chunk> _aliveChunks = new();
	private List<Chunk> _validChunks = new();
	private readonly List<Chunk> _exhaustedChunks = new();
	private List<Chunk> _potentialChunks = new();
	private readonly List<Chunk> _detachedChunks = new();
	private readonly List<Chunk> _chunksToClean = new();

	private readonly List<Bounds> _miningAreaBounds = new();

	private Chunk _startChunk = new();
	private Chunk _lastChunk = new();
	private Bounds _lastBounds = new();
	private Bounds _spawnBounds = new();

	// Neighbors
	private bool _firstNode = true;
	private readonly List<Connection> _connections = new();
	private readonly Dictionary<Chunk, List<Chunk>> _chunkNeighbors = new();

	private readonly float _gridSize = 1.0f;

	private Chunk _chooseChunk1 = null;
	private Chunk _chooseChunk2 = null;

	private MiningArea _miningArea1 = null;
	private MiningArea _miningArea2 = null;

	private MiningArea _lastMiningArea = null;
	private List<MiningArea> _miningAreas = new();

	private bool _canChoose = false;

	private static readonly int s_FindNextPositionAttempts = 100;

	public UnityEvent OnCaveChosen;

	private void Start()
	{
		Random.InitState((int)System.DateTime.Now.Ticks);
		ChunkGenerator chunkGen = _startingChunk.GetComponent<ChunkGenerator>();
		Vector3 chunkCenter = new((chunkGen.Width + (chunkGen.Border * 2)) * .5f, 0, (chunkGen.Height + (chunkGen.Border * 2)) * .5f);
		Bounds chunkBounds = new(chunkCenter, new Vector3(chunkGen.Width + (chunkGen.Border * 2), -1f, chunkGen.Height + (chunkGen.Border * 2)));
		Chunk chunk = new(chunkBounds, chunkGen.Width, chunkGen.Height, chunkGen.Border);
		chunk.Generator = chunkGen;
		Vector3 pos = chunk.Bounds.center - new Vector3(chunk.Bounds.size.x * .5f, 0, chunk.Bounds.size.z * .5f);
		chunkGen.Instantiate(pos, 0);
		_lastChunk = chunk;
		_startChunk = chunk;
		_spawnBounds = chunkBounds;
		_aliveChunks.Add(chunk);
		_firstNode = true;
	}

	public void SpawnCaves()
	{
		foreach (var bounds in _placedMiningBounds)
		{
			if (bounds == null) continue;
			Destroy(bounds);
		}
		_placedMiningBounds.Clear();
		GenerateChoice();
	}
	public void CavesExploded()
	{
		foreach (var chunk in _chunksToClean)
		{
			chunk.Generator.Clean();
			chunk.Generator.Redraw();
		}
		if (_startChunk.Generator != null)
		{
			_startChunk?.Generator.Redraw();
		}
		foreach (var area in _miningAreas)
		{
			if (area == null) continue;
			area.Redraw();
		}
		foreach (var area in _miningAreas)
		{
			if (area == null) continue;
			area.Clean();
		}
		foreach (var bounds in _placedMiningBounds)
		{
			if (bounds == null) continue;
			Destroy(bounds);
		}
		_placedMiningBounds.Clear();

		_chunksToClean.Clear();

		StartCoroutine(RebakeMesh());
	}
	private IEnumerator RebakeMesh()
	{
		yield return null;
		NavMeshRebaker.Instance.Rebake();
	}

	private void GenerateChunk(Choice choice)
	{
		if (_validChunks.Count <= 0)
		{
			HandleNoValidChunks();
			return;
		}

		if (choice == Choice.Potential && _potentialChunks.Count > 0)
		{
			_validChunks.Clear();
			_validChunks.AddRange(_potentialChunks);
			_potentialChunks.Clear();
		}

		Chunk chunk = _validChunks.First();
		_validChunks.RemoveAt(0);

		if (_validChunks.Count <= 0)
		{
			_exhaustedChunks.Add(chunk);
			_aliveChunks.Remove(chunk);
		}

		_aliveChunks.Add(chunk);
		++_amountPlaced;

		if (_firstNode)
		{
			_firstNode = false;
			ConnectChunks(chunk, _startChunk);

			chunk.Generator.MiningEnd();
			_startChunk.Generator.MiningStart();
			_chunksToClean.Add(chunk);
			_chunksToClean.Add(_startChunk);
		}
		else
		{
			ConnectChunks(chunk, _lastChunk);
			chunk.Generator.MiningEnd();
			_lastChunk.Generator.MiningStart();
			_chunksToClean.Add(chunk);
			_chunksToClean.Add(_lastChunk);
		}

		_potentialChunks = GenerateNewBranch(_branchLength - _amountPlaced, chunk.Bounds);
		if (_potentialChunks.Count != _branchLength - _amountPlaced)
		{
			_potentialChunks.Clear();
		}

		DetachAndAddNeighbors(chunk);

		_lastBounds = chunk.Bounds;
		_lastChunk = chunk;
	}
	private void HandleNoValidChunks()
	{
		_exhaustedChunks.Add(_startChunk);
		_aliveChunks.Remove(_startChunk);
		_firstNode = true;
		FindNewStart();
	}
	private void DetachAndAddNeighbors(Chunk chunk)
	{
		List<Chunk> neighbors = _chunkNeighbors[chunk];
		foreach (var neighbor in neighbors)
		{
			if (!_detachedChunks.Contains(neighbor))
			{
				_detachedChunks.Add(neighbor);
			}
			neighbor.Detach();
		}
		chunk.Detach();
		_detachedChunks.Add(chunk);
	}
	public void GenerateChoice()
	{
		if (_validChunks.Count <= 0)
		{
			ResetForNewBranch();
		}
		else
		{
			ChooseAndPlaceChunks();
		}
	}
	private void ResetForNewBranch()
	{
		_amountPlaced = 0;
		_firstNode = true;
		StartCoroutine(ChunkMerging());

		_validChunks = GenerateNewBranch(_branchLength, _startChunk.Bounds);
		_startChunk.Detach();
		_startChunk.Generator.MiningStart();
		_chunksToClean.Add(_startChunk);
		_detachedChunks.Add(_startChunk);
		_potentialChunks.Clear();

		if (_validChunks.Count < _branchLength)
		{
			_validChunks.Clear();
		}
		else
		{
			_chooseChunk1 = _validChunks.First();
			_chooseChunk1.Generator = PlaceChunk(ref _chooseChunk1, -3);
			_miningArea1 = PlaceMiningArea(AddMiningArea(_chooseChunk1, _startChunk));

			_chooseChunk2 = null;
			_miningArea2 = null;
			_canChoose = true;
		}
		if (_validChunks.Count <= 0)
		{
			HandleNoValidChunks();
		}
	}
	private List<Chunk> GenerateNewBranch(int length, Bounds startBounds)
	{
		_lastBounds = startBounds;
		List<Chunk> branchChunks = new();

		for (int i = 0; i < length; i++)
		{
			int minSize = _amountPlaced + i * _minSizeGrowth;
			int maxSize = _amountPlaced + i * _maxSizeGrowth;

			int chunkWidth = Mathf.RoundToInt(Random.Range(_minChunkSize + minSize, _maxChunkSize + maxSize + 1) * 0.5f) * 2;
			int chunkHeight = Mathf.RoundToInt(Random.Range(_minChunkSize + minSize, _maxChunkSize + maxSize + 1) * 0.5f) * 2;
			int doubleBorder = _border * 2;

			Vector3 chunkCenter = FindNextPosition(chunkWidth + doubleBorder, chunkHeight + doubleBorder);

			if (chunkCenter != Vector3.zero)
			{
				chunkCenter = SnapToGrid(chunkCenter);
				Bounds chunkBounds = new(chunkCenter, new Vector3(chunkWidth + doubleBorder, 0, chunkHeight + doubleBorder));
				Chunk chunk = new(chunkBounds, chunkWidth, chunkHeight, _border);

				_lastBounds = chunkBounds;
				_lastChunk = chunk;
				branchChunks.Add(chunk);
				_potentialChunks.Add(chunk);
				if (i == length-1)
					chunk.SetLastChunk(true);
			}
		}

		return branchChunks;
	}
	private Bounds AddMiningArea(Chunk chunk1, Chunk chunk2)
	{
		Vector3 closestPointOnChunk1 = chunk1.Bounds.ClosestPoint(chunk2.Bounds.center);
		Vector3 closestPointOnChunk2 = chunk2.Bounds.ClosestPoint(chunk1.Bounds.center);

		float dist = Vector3.Distance(chunk1.Bounds.center, chunk2.Bounds.center);

		Vector3 middlePoint = (closestPointOnChunk1 + closestPointOnChunk2) / 2;
		Vector3 boundSize = new Vector3(_minChunkSize, 0f, _minChunkSize);
		Vector3 areaSize = new Vector3(Mathf.Ceil(dist+_border), 1f, Mathf.Ceil(dist + _border));

		Bounds newBounds = new(middlePoint, boundSize);

		Vector3 center = SnapToGrid(middlePoint) - new Vector3(_gridSize * .5f, 0, _gridSize * .5f);

		GameObject go = Instantiate(_miningAreaObject, center, Quaternion.identity);
		go.transform.localScale = areaSize;
		go.SetActive(false);
		_placedMiningBounds.Add(go);

		_miningAreaBounds.Add(newBounds);
		return newBounds;
	}

	private Vector3 FindNextPosition(int chunkWidth, int chunkHeight)
	{
		for (int i = 0; i < s_FindNextPositionAttempts; i++)
		{
			Vector3 adjacentPosition = FindAdjacentPosition(_lastBounds, chunkWidth, chunkHeight);
			if (adjacentPosition != Vector3.zero)
			{
				return adjacentPosition;
			}
		}
		return Vector3.zero;
	}
	private Vector3 FindAdjacentPosition(Bounds existingChunkBounds, int chunkWidth, int chunkHeight)
	{
		Vector3 existingCenter = existingChunkBounds.center;
		const float half = 0.5f;

		float minX = -existingChunkBounds.size.x * half - chunkWidth * half;
		float maxX = existingChunkBounds.size.x * half + chunkWidth * half;
		float minY = -existingChunkBounds.size.z * half - chunkHeight * half;
		float maxY = existingChunkBounds.size.z * half + chunkHeight * half;

		float adjustedMinX = Mathf.Max(minX, minX + _minTouchingArea);
		float adjustedMaxX = Mathf.Min(maxX, maxX - _minTouchingArea);
		float adjustedMinY = Mathf.Max(minY, minY + _minTouchingArea);
		float adjustedMaxY = Mathf.Min(maxY, maxY - _minTouchingArea);

		List<int> sidesToCheck = new() { 0, 1, 2, 3 };
		ShuffleList(sidesToCheck);

		foreach (int sideToUse in sidesToCheck)
		{
			Vector3 newPosition = existingCenter;

			newPosition += sideToUse switch
			{
				// Top side
				0 => new Vector3(Random.Range(adjustedMinX, adjustedMaxX + 1), 0, maxY),
				// Right side
				1 => new Vector3(maxX, 0, Random.Range(adjustedMinY, adjustedMaxY + 1)),
				// Bottom side
				2 => new Vector3(Random.Range(adjustedMinX, adjustedMaxX + 1), 0, minY),
				// Left side
				_ => new Vector3(minX, 0, Random.Range(adjustedMinY, adjustedMaxY + 1)),
			};

			// Check for overlapping with existing chunks
			bool overlaps = CheckForOverlaps(newPosition, chunkWidth, chunkHeight);

			// If no overlap is found, return the newPosition
			if (!overlaps)
			{
				return newPosition;
			}
		}
		return Vector3.zero;
	}
	private bool CheckForOverlaps(Vector3 position, int chunkWidth, int chunkHeight)
	{
		bool overlapsAlive = _aliveChunks.Any(chunk => CheckBoundsOverlap(position, chunkWidth, chunkHeight, chunk.Bounds));
		bool overlapsValid = _validChunks.Any(chunk => CheckBoundsOverlap(position, chunkWidth, chunkHeight, chunk.Bounds));
		bool overlapsExhausted = _exhaustedChunks.Any(chunk => CheckBoundsOverlap(position, chunkWidth, chunkHeight, chunk.Bounds));
		bool overlapsPotential = _potentialChunks.Any(chunk => CheckBoundsOverlap(position, chunkWidth, chunkHeight, chunk.Bounds));

		return overlapsAlive || overlapsValid || overlapsExhausted || overlapsPotential;
	}

	private void ChooseAndPlaceChunks()
	{
		_miningAreaBounds.Clear();

		int r = Random.Range(0, 3);
		_chooseChunk1 = _validChunks.First();
		_chooseChunk1.Generator = PlaceChunk(ref _chooseChunk1, r);

		_miningArea1 = PlaceMiningArea(AddMiningArea(_chooseChunk1, _lastChunk));
		_chooseChunk1.Generator.Redraw();

		if (_potentialChunks.Count > 0)
		{
			r = Random.Range(0, 3);
			r = -r;
			_chooseChunk2 = _potentialChunks.First();
			_chooseChunk2.Generator = PlaceChunk(ref _chooseChunk2, r);
			_miningArea2 = PlaceMiningArea(AddMiningArea(_chooseChunk2, _lastChunk));
			_chooseChunk2.Generator.Redraw();
		}
		else
		{
			_chooseChunk2 = null;
			_miningArea2 = null;
		}

		_lastChunk.Generator.Clean();

		_canChoose = true;
	}
	public bool Choose(Ray ray)
	{
		if (_canChoose)
		{
			Choice choice = CheckChoice(ray);
			if (choice >= 0)
			{
				_canChoose = false;
				Submit(choice);
				return true;
			}
			else
			{
				return false;
			}
		}
		return false;
	}
	private Choice CheckChoice(Ray ray)
	{
		List<MiningArea> areas = new();
		areas.AddRange(_miningAreas.Union(areas));
		_miningAreas.Clear();
		if (_canChoose)
		{
			if (_validChunks.Count > 0 && _validChunks.First().Bounds.IntersectRay(ray))
			{
				_miningAreas = areas;
				return Choice.Valid;
			}
			else if (_potentialChunks.Count > 0 && _potentialChunks.First().Bounds.IntersectRay(ray))
			{
				_miningAreas = areas;
				return Choice.Potential;
			}
			else
			{
				_miningAreas = areas;
				return Choice.Invalid; // Not inside either chunk
			}
		}
		else
			return Choice.Invalid; // Can't Choose
	}

	public void Submit(int choice)
	{
		Submit((Choice)choice);
	}
	private void Submit(Choice choice)
	{
		_lastMiningArea?.Clean();
		if (choice == Choice.Valid)
		{
			_highlightObjects[0].GetComponent<Highlight>().Pick();
			_highlightObjects.Remove(_highlightObjects[0]);
			DestroyGenerator(_chooseChunk2);
			if (_miningArea2)
			{
				Destroy(_miningArea2.gameObject);
			}
			if (_placedMiningBounds.Count > 0)
			{
				GameObject bounds = _placedMiningBounds[0];

				if (bounds != null)
				{
					bounds.SetActive(true);
				}
			}
			_miningArea1.Redraw();
			_lastMiningArea = _miningArea1;
		}
		else if (choice == Choice.Potential)
		{
			_highlightObjects[1].GetComponent<Highlight>().Pick();
			_highlightObjects.Remove(_highlightObjects[1]);
			DestroyGenerator(_chooseChunk1);
			if (_miningArea1)
			{
				Destroy(_miningArea1.gameObject);
			}
			_miningArea2.Redraw();
			if (_placedMiningBounds.Count > 0)
			{
				GameObject bounds = _placedMiningBounds[1];

				if (bounds != null)
				{
					bounds.SetActive(true);
				}
			}
			_lastMiningArea = _miningArea2;
		}
		_miningArea1 = null;
		_miningArea2 = null;
		_miningAreas.Add(_lastMiningArea);
		GenerateChunk(choice);
		OnCaveChosen.Invoke();
		foreach (var go in _highlightObjects)
		{
			Destroy(go);
		}
		_highlightObjects.Clear();
		foreach (var chunk in _chunksToClean)
		{
			chunk.Generator.Redraw();
		}
	}
	private void DestroyGenerator(Chunk chunk)
	{
		if (chunk != null && chunk.Generator != null)
		{
			Destroy(chunk.Generator.gameObject);
		}
	}
	private ChunkGenerator PlaceChunk(ref Chunk chunk, int level)
	{
		// Original chunk position calculation
		Vector3 pos = chunk.Bounds.center - new Vector3(chunk.Bounds.size.x * 0.5f, 0, chunk.Bounds.size.z * 0.5f);

		// Instantiate the main chunk
		GameObject chunkObject = Instantiate(_chunkGenerator, pos, Quaternion.identity);
		if (chunkObject.TryGetComponent<ChunkGenerator>(out var newChunk))
		{
			newChunk.Width = chunk.ChunkWidth;
			newChunk.Height = chunk.ChunkHeight;
			newChunk.Border = chunk.BorderSize;
			newChunk.IsLastChunk = chunk.IsLastChunk;
			newChunk.Instantiate(pos, level);
		}

		chunkObject.name = chunk.Bounds.center.ToString();

		Vector3 center = SnapToGrid(chunk.Bounds.center) - new Vector3(_gridSize * .5f, 0, _gridSize * .5f);

		GameObject highLIGHT = Instantiate(_highlightObject, center, Quaternion.identity);

		Vector3 scale = new(chunk.ChunkWidth + _border * 2, highLIGHT.transform.localScale.y, chunk.ChunkHeight + _border * 2);
		highLIGHT.transform.localScale = scale;
		if (!_firstNode)
		{
			Vector3[] centers = { chunk.Bounds.center, _lastChunk.Bounds.center };
			highLIGHT.GetComponent<Highlight>().Connect(centers);
		}
		else
		{
			Vector3[] centers = { chunk.Bounds.center, _startChunk.Bounds.center };
			highLIGHT.GetComponent<Highlight>().Connect(centers);
		}
		_highlightObjects.Add(highLIGHT);

		return newChunk;
	}

	private MiningArea PlaceMiningArea(Bounds area)
	{
		Vector3 pos = SnapToGrid(area.center) - new Vector3(area.size.x * .5f, 0, area.size.z * .5f);
		GameObject miningAreaObject = Instantiate(_miningArea, pos, Quaternion.identity);
		if (miningAreaObject.TryGetComponent<MiningArea>(out var newMiningArea))
		{
			newMiningArea.Size = _minChunkSize;
			newMiningArea.Instantiate(pos);
		}

		miningAreaObject.name = area.center.ToString();
		return newMiningArea;
	}
	private void ConnectChunks(Chunk chunk1, Chunk chunk2)
	{
		_connections.Add(new Connection(chunk1, chunk2));

		// Create a connection in the dictionary for chunk1.
		if (!_chunkNeighbors.ContainsKey(chunk1))
		{
			_chunkNeighbors[chunk1] = new List<Chunk>();
		}
		_chunkNeighbors[chunk1].Add(chunk2);

		// Create a connection in the dictionary for chunk2.
		if (!_chunkNeighbors.ContainsKey(chunk2))
		{
			_chunkNeighbors[chunk2] = new List<Chunk>();
		}
		_chunkNeighbors[chunk2].Add(chunk1);
	}
	private void FindNewStart()
	{
		float closestDistanceSquared = float.MaxValue;
		Chunk newStartChunk = null;
		Vector3 spawnCenter = _spawnBounds.center;

		Parallel.ForEach(_aliveChunks, candidateChunk =>
		{
			Vector3 candidateCenter = candidateChunk.Bounds.center;
			float distanceSquared = Vector3.SqrMagnitude(candidateCenter - spawnCenter);

			if (distanceSquared < closestDistanceSquared)
			{
				closestDistanceSquared = distanceSquared;
				newStartChunk = candidateChunk;
			}
		});
		_detachedChunks.Add(_startChunk);
		_startChunk = newStartChunk;
		_lastChunk = newStartChunk;
		newStartChunk.Generator.MiningStart();
		GenerateChoice();
	}
	private IEnumerator ChunkMerging()
	{
		List<Chunk> branchChunks = new();
		branchChunks.AddRange(_detachedChunks.Union(branchChunks));
		_detachedChunks.Clear();
		List<MiningArea> branchAreas = new();
		branchAreas.AddRange(_miningAreas.Union(branchAreas));
		_miningAreas.Clear();
		yield return null;
		
		// Disabled for now, floor meshes are causing issues with the pathfinding 
		// FIX THIS

		//	foreach (Chunk chunk in branchChunks)
		//	{
		//		yield return new WaitForSeconds(2f);
		//		if (chunk.Generator && chunk != _startChunk)
		//		{
		//			chunk.Merge();
		//			StartCoroutine(RebakeMesh());
		//		}
		//	}
		//	foreach (MiningArea area in branchAreas)
		//	{
		//		area.Solidify();
		//	}
	}
	private bool CheckBoundsOverlap(Vector3 position, int chunkWidth, int chunkHeight, Bounds bounds)
	{
		const float half = .5f;

		float minX = position.x - chunkWidth * half;
		float maxX = position.x + chunkWidth * half;
		float minY = position.z - chunkHeight * half;
		float maxY = position.z + chunkHeight * half;

		int overlapEdges = 0;

		if (maxX > bounds.min.x && minX < bounds.max.x) // Check horizontal overlap
		{
			++overlapEdges;
		}

		if (maxY > bounds.min.z && minY < bounds.max.z) // Check vertical overlap
		{
			++overlapEdges;
		}

		return overlapEdges >= 2;
	}
	private Vector3 SnapToGrid(Vector3 position)
	{
		position.x = Mathf.Round(position.x / _gridSize) * _gridSize;
		position.y = Mathf.Round(position.y / _gridSize) * _gridSize;
		position.z = Mathf.Round(position.z / _gridSize) * _gridSize;
		return position;
	}
	private void ShuffleList<T>(List<T> list)
	{
		int n = list.Count;
		while (n > 1)
		{
			n--;
			int k = Random.Range(0, n + 1);
			(list[n], list[k]) = (list[k], list[n]);
		}
	}
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		foreach (Chunk chunk in _aliveChunks)
		{
			Gizmos.DrawWireCube(chunk.Bounds.center, chunk.Bounds.size);
		}

		Gizmos.color = Color.blue;
		foreach (Chunk chunk in _validChunks)
		{
			Gizmos.DrawWireCube(chunk.Bounds.center, chunk.Bounds.size);
		}

		Gizmos.color = Color.red;
		foreach (Chunk chunk in _exhaustedChunks)
		{
			Gizmos.DrawWireCube(chunk.Bounds.center, chunk.Bounds.size);
		}

		Gizmos.color = Color.yellow;
		foreach (Connection connection in _connections)
		{
			Gizmos.DrawLine(connection.chunk1.Bounds.center, connection.chunk2.Bounds.center);
		}

		Gizmos.color = Color.magenta;
		foreach (Chunk chunk in _potentialChunks)
		{
			Gizmos.DrawWireCube(chunk.Bounds.center, chunk.Bounds.size);
		}

		Gizmos.color = Color.cyan;
		foreach (Bounds bound in _miningAreaBounds)
		{
			Gizmos.DrawWireCube(bound.center, bound.size);
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelGenerator))]
public class LevelGenerator_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		LevelGenerator script = (LevelGenerator)target;

		if (GUILayout.Button("Generate"))
		{
			script.SpawnCaves();
			script.Submit(0);
		}
		if (GUILayout.Button("Generate 3"))
		{
			for (int i = 0; i < 3; i++)
			{
				script.GenerateChoice();
				script.Submit(0);
			}
		}
		if (GUILayout.Button("Generate 10"))
		{
			for (int i = 0; i < 10; i++)
			{
				script.GenerateChoice();
				script.Submit(0);
			}
		}
		if (GUILayout.Button("Generate 100"))
		{
			for (int i = 0; i < 100; i++)
			{
				script.GenerateChoice();
				script.Submit(0);
			}
		}
		if (GUILayout.Button("Generate 1000"))
		{
			for (int i = 0; i < 1000; i++)
			{
				script.GenerateChoice();
				script.Submit(0);
			}
		}
		if (GUILayout.Button("Choose"))
		{
			script.GenerateChoice();
		}
		if (GUILayout.Button("Choose 1"))
		{
			script.Submit(0);
		}
		if (GUILayout.Button("Choose 2"))
		{
			script.Submit(1);
		}
		if (GUILayout.Button("Choose and pick 1"))
		{
			script.GenerateChoice();
			script.Submit(0);
		}
		if (GUILayout.Button("Choose and pick 2"))
		{
			script.GenerateChoice();
			script.Submit(1);
		}
	}
}
#endif