using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
	[Header("Chunk Settings")]
	[SerializeField] private int _width;
	[SerializeField] private int _height;
	[SerializeField] private int _border;
	[SerializeField] private float _wallHeight = 1f;
	[SerializeField] private float _yOffset = .5f;
	[SerializeField] private bool _generate = true;

	[Header("Tile Prefabs")]
	[SerializeField] private GameObject _wall;
	[SerializeField] private GameObject _ground;
	[SerializeField] private GameObject _pillar;

	[SerializeField] private GameObject[] _chasmObjects;

	[SerializeField, Range(0, 1)] private float _MaxPercentageBlocked = 0.1f;
	[SerializeField] private GameObject _blockerObject;

	[Header("Mining Prefabs")]
	[SerializeField] private GameObject _miningStart;
	[SerializeField] private GameObject _miningEnd;

	[Header("POI")]
	[SerializeField] private int _minCaveSize = 18;
	private int _caveSize = 0;
	[SerializeField] private GameObject _nestSpawnerObject;
	[SerializeField] private GameObject _rootNestSpawnerObject;
	[SerializeField] private GameObject _resourceTile;
	[SerializeField] private GameObject _wreckObject;
	[Tooltip("Speed at which new caves gain extra budget")]
	[SerializeField] private float _levelIncrease = .001f;
	[Tooltip("How much the level inscreases the budget - budget = level * blm")]
	[SerializeField] private float _budgetLevelMultiplier = 10f;
	[Tooltip("Minimum budget of a cave")]
	[SerializeField] private int _minBudget = 30;
	[Tooltip("Amount of budget fluxation per level - % of budget")]
	[SerializeField] private float _budgetVariation = 0.1f;
	[SerializeField] private int _wreckCost = 6;
	[SerializeField] private int _minWrecks = 1;
	[SerializeField] private int _maxWrecks = 3;
	[Tooltip("Budget cost per recourse value")]
	[SerializeField] private float _recourseCostMultiplier = 1;
	private int _levelVariation = 0;
	[HideInInspector] public static float s_Level = 0;
	private static bool s_canIncrease = true;
	private List<Vector2Int> _placedSpots = new List<Vector2Int>();

	private readonly List<GameObject> _spawnedObjects = new();

	[Header("Generators")]
	[SerializeField] private CellularAutomata _caveGenerator;
	[SerializeField] private CellularAutomata _fallbackCaveGenerator;
	[SerializeField] private ObstacleLayers _obstacleLayers;
	[SerializeField] private ObstacleLayers _fallbackObstacleLayers;

	private bool[,] _cellmap;
	private bool[,] _cellmapWithBorders;
	private bool[,] _cellmapMiddleChunk;
	private bool[,] _visited;

	private Vector2Int _middleCell;

	private bool _isMerged = false;
	private bool _isLastChunk = false;
	private bool _isGenerated = false;

	private List<GameObject> _tempObjects = new();

	public int Width { get => _width; set => _width = value; }
	public int Height { get => _height; set => _height = value; }
	public int Border { get => _border; set => _border = value; }
	public bool IsLastChunk { get => _isLastChunk; set => _isLastChunk = value; }

	private static readonly int s_MaxVerticesPerBatch = 65535; //Max amounts of verts in unity

	private void OnDestroy()
	{
		foreach (var go in _spawnedObjects)
		{
			Destroy(go);
		}
	}

	private void Awake()
	{
		Random.InitState((int)System.DateTime.Now.Ticks);
	}

	#region Generating
	public void Instantiate(Vector3 pos, int level)
	{
		_levelVariation = level;
		if (s_canIncrease)
		{
			s_Level += _levelIncrease;
			s_canIncrease = false;
			StartCoroutine(ResetCanIncrease());
		}

		if (!_generate)
		{
			GenerateMap(pos);
			UpdateBorderMap();
			//InstantiateOnWallNeighbors(_miningStart);
			StartCoroutine(RebakeMesh());
			return;
		}

		StartCoroutine(TryGenerate(pos));
	}

	private IEnumerator TryGenerate(Vector3 pos)
	{
		bool generated = false;
		int attempts = 10;

		for (int attempt = 0; attempt < attempts; attempt++)
		{
			ClearChunk();
			generated = GenerateMap(pos);
			if (generated)
			{
				break;
			}
			yield return null;
		}

		if (!generated)
		{
			Debug.LogWarning("Can't Generate after multiple attempts, using fallback map");
			_obstacleLayers = _fallbackObstacleLayers;
			for (int attempt = 0; attempt < attempts; attempt++)
			{
				ClearChunk();
				generated = GenerateMap(pos);
				if (generated)
				{
					break;
				}
				yield return null;
			}

			if (!generated)
			{
				Debug.LogWarning("Can't generate even with fallback map, using fallback automata");

				_caveGenerator = _fallbackCaveGenerator;
				_minCaveSize = 9;

				for (int attempt = 0; attempt < attempts; attempt++)
				{
					ClearChunk();
					generated = GenerateMap(pos);
					if (generated)
					{
						break;
					}
					yield return null;
				}
				if (!generated)
				{
					Debug.LogError("Can't generate even with fallback automata, this should never happen");
				}
			}
		}

		DrawCells();
		SpawnPOI();

		yield return null;
		_isGenerated = true;
		DrawCells();
		StartCoroutine(RebakeMesh());
	}


	private IEnumerator ResetCanIncrease()
	{
		yield return new WaitForEndOfFrame();
		s_canIncrease = true;
	}

	public void ClearChunk()
	{
		foreach (Transform child in transform)
		{
			Destroy(child.gameObject);
		}
	}

	bool GenerateMap(Vector3 pos)
	{
		int mapWidthWithBorders = _width + 2 * _border;
		int mapHeightWithBorders = _height + 2 * _border;
		_cellmapWithBorders = new bool[mapWidthWithBorders, mapHeightWithBorders];
		_visited = new bool[mapWidthWithBorders, mapHeightWithBorders];

		for (int x = 0; x < _visited.GetLength(0); x++)
		{
			for (int y = 0; y < _visited.GetLength(1); y++)
			{
				_visited[x, y] = false;
			}
		}

		if (_generate)
		{
			Generate(pos);
		}
		else
		{
			UpdateBorderMap();
		}

		int centerX = _width / 2 + _border;
		int centerY = _height / 2 + _border;
		int foundX = centerX;
		int foundY = centerY;
		int cellcount = 0;
		int attempts = 3;
		int i = 0;

		for (i = 0; i < attempts; i++)
		{
			if (_cellmapWithBorders[foundX, foundY] || IsChasm(foundX, foundY))
			{
				// Find closest _cellmapWithBorders[x,y] == false to center
				float closestDistance = float.MaxValue;
				for (int x = 0; x < _width + _border * 2; x++)
				{
					for (int y = 0; y < _height + _border * 2; y++)
					{
						if (!_cellmapWithBorders[x, y] && !IsChasm(x, y) && !_visited[x, y])
						{
							float distance = Vector2Int.Distance(new Vector2Int(x, y), new Vector2Int(centerX, centerY));
							if (distance < closestDistance)
							{
								closestDistance = distance;
								foundX = x;
								foundY = y;
							}
						}
					}
				}
			}
			var emptyarea = ExploreEmptyArea(foundX, foundY);
			cellcount = emptyarea.Item2;
			if (cellcount >= _minCaveSize)
			{
				_caveSize = cellcount;
				_cellmapMiddleChunk = emptyarea.Item1;
				return true;
			}
			else
			{
				Debug.LogWarning(cellcount +" - " + foundX + ", " + foundY);
			}
		}

		if (i >= attempts)
		{
			Debug.LogWarning("Can't Generate itteration");
			return false;
		}
		return false;
	}
	private IEnumerator RebakeMesh()
	{
		yield return null;
		NavMeshRebaker.Instance.Rebake();
	}

	private void Generate(Vector3 pos)
	{
		_cellmap = _caveGenerator.Generate(_width, _height);

		UpdateCellmap(pos);

		Draw(pos);

		UpdateBorderMap();

		DrawCells();
	}
	private void DrawCells()
	{
		var tiles = GetComponentsInChildren<Tile>();
		foreach (var tile in tiles)
		{
			tile.Draw();
		}
	}
	private void UpdateCellmap(Vector3 pos)
	{
		int intPosX = (int)pos.x;
		int intPosZ = (int)pos.z;


		for (int x = -_border; x < _width + _border; x++)
		{
			for (int y = -_border; y < _height + _border; y++)
			{
				bool isInsideChunk = x >= 0 && x < _width && y >= 0 && y < _height;

				if (isInsideChunk)
				{
					int mapX = intPosX + x;
					int mapY = intPosZ + y;
					Vector2Int coords = _obstacleLayers.GetMapCoords(mapX + _border, mapY + _border);

					// Check if there is a pillar at this position
					bool hasPillar = !_obstacleLayers.PillarMap[coords.x, coords.y];

					// Check if there is a chasm at this position
					bool hasChasm = !_obstacleLayers.ChasmMap[coords.x, coords.y];

					if (hasChasm)
					{
						bool hasLeftNeighbor = x - 1 >= 0;
						bool hasRightNeighbor = x + 1 < _width;
						bool hasUpNeighbor = y + 1 < _height;
						bool hasDownNeighbor = y - 1 >= 0;

						if (hasLeftNeighbor) _cellmap[x - 1, y] = false;
						if (hasRightNeighbor) _cellmap[x + 1, y] = false;
						if (hasUpNeighbor) _cellmap[x, y + 1] = false;
						if (hasDownNeighbor) _cellmap[x, y - 1] = false;
					}

					if (hasPillar)
					{
						// Check if there are adjacent cells and add walls
						bool hasLeftNeighbor = x - 1 >= 0;
						bool hasRightNeighbor = x + 1 < _width;
						bool hasUpNeighbor = y + 1 < _height;
						bool hasDownNeighbor = y - 1 >= 0;

						if (hasLeftNeighbor) _cellmap[x - 1, y] = true;
						if (hasRightNeighbor) _cellmap[x + 1, y] = true;
						if (hasUpNeighbor) _cellmap[x, y + 1] = true;
						if (hasDownNeighbor) _cellmap[x, y - 1] = true;
					}
				}
			}
		}
	}
	private void InstantiateAndName(GameObject prefab, Vector3 position)
	{
		var go = Instantiate(prefab, position, Quaternion.identity, transform);
		//go.name = position.ToString();
	}
	private void UpdateBorderMap()
	{
		for (int x = 0; x < _width + _border * 2; x++)
		{
			for (int y = 0; y < _height + _border * 2; y++)
			{
				Vector3 offset = new(transform.position.x + x, transform.position.y + _yOffset, transform.position.z + y);

				const float checkSize = 0.2f;
				Collider[] colliders = Physics.OverlapSphere(offset, checkSize);
				if (colliders.Length > 0)
				{
					bool hasTile = false;
					foreach (Collider collider in colliders)
					{
						if (collider.GetComponentInParent<Tile>() != null)
						{
							hasTile = true;
							break;
						}
					}
					_cellmapWithBorders[x, y] = hasTile;
				}
				else
				{
					_cellmapWithBorders[x, y] = false;
				}
			}
		}
	}
	private void Draw()
	{
		Draw(gameObject.transform.position);
	}
	private void Draw(Vector3 pos)
	{
		int intPosX = (int)pos.x;
		int intPosZ = (int)pos.z;

		UpdateCellmap(pos);

		for (int x = -_border; x < _width + _border; x++)
		{
			for (int y = -_border; y < _height + _border; y++)
			{
				int mapX = intPosX + x;
				int mapY = intPosZ + y;
				bool isInsideChunk = x >= 0 && x < _width && y >= 0 && y < _height;
				Vector2Int coords = _obstacleLayers.GetMapCoords(mapX + _border, mapY + _border);

				Vector3 offset = new(transform.position.x + x + _border, transform.position.y + _yOffset, transform.position.z + y + _border);
				Vector3 floorPos = offset + new Vector3(0, -1, 0);
				Vector3 wallPos = offset;
				Vector3 pillarPos = offset + new Vector3(0, _wallHeight, 0);

				// Chasms
				if (_obstacleLayers.ChasmMap[coords.x, coords.y])
				{
					InstantiateAndName(_ground, floorPos);
				}
				else if (_chasmObjects.Length > 0)
				{
					if (Random.Range(0, 11) == 0)
					{
						GameObject randomChasmObject = _chasmObjects[Random.Range(0, _chasmObjects.Length)];
						InstantiateAndName(randomChasmObject, floorPos);
						continue;
					}
				}

				// Walls
				bool currentCell = true;
				if (isInsideChunk)
				{
					currentCell = _cellmap[x, y];
				}
				if (!_obstacleLayers.PillarMap[coords.x, coords.y])
				{
					InstantiateAndName(_wall, wallPos);
				}
				else if (currentCell && _obstacleLayers.ChasmMap[coords.x, coords.y])
				{
					InstantiateAndName(_wall, wallPos);
					continue;
				}

				// Pillars
				if (!_obstacleLayers.PillarMap[coords.x, coords.y])
				{
					InstantiateAndName(_pillar, pillarPos);
				}
			}
		}

		DrawCells();
		_isMerged = false;
	}
	private void SpawnPOI()
	{
		float firstcaveLevel = _levelIncrease;

		Debug.Log(firstcaveLevel);
		Debug.Log(s_Level);

		Debug.Log(gameObject.transform.position + " - " + s_Level);
		int budget = (int)((1+s_Level) * _budgetLevelMultiplier);
		Debug.Log("budget (1+s_Level) * _budgetLevelMultiplier: " + budget);
		budget += _minBudget;
		Debug.Log("budget _minBudget: " + budget);
		int budgetVariation = Mathf.Clamp(_levelVariation,-2, 2);
		if (((s_Level) / _levelIncrease) - budgetVariation < 0)
		{
			budgetVariation = 0;
		}

		if (s_Level == firstcaveLevel)
		{
			budgetVariation = 0;
		}

		_budgetVariation *= budgetVariation;
		Debug.Log("budgetvariation: " + _budgetVariation);
		budget += (int)(budget * _budgetVariation);

		Debug.Log("end budget: " + budget);

		if (_nestSpawnerObject && !_isLastChunk)
		{
			Vector2Int pos =  FindNearest3x3FreeSpace();
			var go = Instantiate(_nestSpawnerObject, new(transform.position.x + pos.x, transform.position.y, transform.position.z + pos.y), Quaternion.identity);
			_spawnedObjects.Add(go);

			go.SendMessage("Initialize", _levelVariation, SendMessageOptions.DontRequireReceiver);
		}
		if (_rootNestSpawnerObject && _isLastChunk)
		{
			Vector2Int pos = FindNearest3x3FreeSpace();
			var go = Instantiate(_rootNestSpawnerObject, new(transform.position.x + pos.x, transform.position.y, transform.position.z + pos.y), Quaternion.identity);
			_spawnedObjects.Add(go);

			go.SendMessage("Initialize", _levelVariation, SendMessageOptions.DontRequireReceiver);
		}
		if (_wreckObject)
		{
			int amount = Random.Range(_minWrecks, _maxWrecks + 1);
			if (s_Level == firstcaveLevel)
			{
				amount = 2;
				Debug.Log("Spawning 2 wrecks because first cave");
			}
			for (int i = 0; i < amount; i++)
			{
				Vector2Int pos = FindRandomFreeSpace(_cellmapMiddleChunk);
				var go = Instantiate(_wreckObject, new(transform.position.x + pos.x, transform.position.y + _yOffset, transform.position.z + pos.y), Quaternion.identity);
				_spawnedObjects.Add(go);

				budget -= _wreckCost;
				Debug.Log("budget - wreck: " + budget);
			}
		}
		if (_resourceTile)
		{
			int cost = Random.Range(1, 4);
			if (s_Level == firstcaveLevel)
			{
				cost = 1;
				Debug.Log("ResourceCost 1 because first cave");
			}
			cost *= 2;
			cost = Mathf.CeilToInt(cost * _recourseCostMultiplier);

			while (budget >= cost)
			{
				Vector2Int pos = FindRandomFreeSpace(_cellmapMiddleChunk);
				var go = Instantiate(_resourceTile, new(transform.position.x + pos.x, transform.position.y + _yOffset, transform.position.z + pos.y), Quaternion.identity);
				_spawnedObjects.Add(go);

				budget -= cost;
				Debug.Log("budget - resource: " + budget);
				go.SendMessage("Initialize", cost, SendMessageOptions.DontRequireReceiver);
			}
		}
		SpawnBlockers();
	}

	private void SpawnBlockers()
	{
		if (_blockerObject)
		{
			int MaxAmount = (int)((_caveSize - _spawnedObjects.Count) * _MaxPercentageBlocked);
			int amount = Random.Range(0, MaxAmount);

			for (int i = 0; i < amount; i++)
			{
				Vector2Int pos = FindRandomFreeSpace(_cellmapWithBorders);
				Vector3 position = new(transform.position.x + pos.x, transform.position.y + _yOffset, transform.position.z + pos.y);
				var go = Instantiate(_blockerObject, position, Quaternion.identity);
				_spawnedObjects.Add(go);
			}
		}
	}

	private Vector2Int FindNearest3x3FreeSpace()
	{
		Vector2Int center = new Vector2Int(_middleCell.x, _middleCell.y);
		Vector2Int nearest = new Vector2Int(_middleCell.x, _middleCell.y);
		float closestDistance = float.MaxValue;

		for (int x = 0; x <= _cellmapMiddleChunk.GetLength(0); x++)
		{
			for (int y = 0; y <= _cellmapMiddleChunk.GetLength(1); y++)
			{
				if (x >= 0 && x < _cellmapMiddleChunk.GetLength(0) &&
					y >= 0 && y < _cellmapMiddleChunk.GetLength(1) &&
					!_cellmapMiddleChunk[x, y] && !IsChasm(x, y))
				{
					bool allNeighborsAreFree = true;
					for (int dx = -1; dx <= 1; dx++)
					{
						for (int dy = -1; dy <= 1; dy++)
						{
							int neighborX = x + dx;
							int neighborY = y + dy;

							if (neighborX >= 0 && neighborX < _cellmapMiddleChunk.GetLength(0) &&
								neighborY >= 0 && neighborY < _cellmapMiddleChunk.GetLength(1) &&
								(_cellmapMiddleChunk[neighborX, neighborY] || IsChasm(neighborX, neighborY)))
							{
								allNeighborsAreFree = false;
								break;
							}
						}
						if (!allNeighborsAreFree) break;
					}

					if (allNeighborsAreFree)
					{
						float distance = Vector2Int.Distance(new Vector2Int(x, y), center);
						if (distance < closestDistance)
						{
							closestDistance = distance;
							nearest.x = x;
							nearest.y = y;
						}
					}
				}
			}
		}
		return nearest;
	}
	private Vector2Int FindRandomFreeSpace(bool[,] map)
	{
		System.Random random = new System.Random();
		bool found = false;
		int rows = map.GetLength(0);
		int cols = map.GetLength(1);
		List<Vector2Int> checkedSpots = new List<Vector2Int>();
		checkedSpots.AddRange(_placedSpots);

		while (!found && checkedSpots.Count < rows * cols)
		{
			int randomRow, randomCol;
			Vector2Int checkedSpot = new Vector2Int();
			do
			{
				randomRow = random.Next(0, rows);
				randomCol = random.Next(0, cols);
				checkedSpot = new Vector2Int(randomRow, randomCol);
			} while (checkedSpots.Contains(checkedSpot));

			checkedSpots.Add(checkedSpot);

			if (!map[randomRow, randomCol] && !IsChasm(randomRow, randomCol))
			{
				// Found a random false value
				found = true;
				_placedSpots.Add(checkedSpot);
				return checkedSpot;
			}
		}
		return new Vector2Int(_middleCell.x, _middleCell.y);
	}

	private bool IsChasm(int x, int y)
	{
		int intPosX = (int)transform.position.x;
		int intPosZ = (int)transform.position.z;

		int mapX = intPosX + x;
		int mapY = intPosZ + y;
		Vector2Int coords = _obstacleLayers.GetMapCoords(mapX, mapY);

		return !_obstacleLayers.ChasmMap[coords.x, coords.y];
	}

	//	private static void ApplyFunctionToGrid(int width, int height, int border, Action<int, int> action)
	//	{
	//		for (int x = -border; x < width + border; x++)
	//		{
	//			for (int y = -border; y < height + border; y++)
	//			{
	//				action(x, y);
	//			}
	//		}
	//	}
	#endregion

	#region Mining
	private void InstantiateOnWallNeighbors(GameObject objectToInstantiate)
	{
		int intPosX = (int)transform.position.x;
		int intPosZ = (int)transform.position.z;
		for (int x = 0; x < _width + _border * 2; x++)
		{
			for (int y = 0; y < _height + _border * 2; y++)
			{
				int mapX = intPosX + x;
				int mapY = intPosZ + y;
				Vector2Int coords = _obstacleLayers.GetMapCoords(mapX, mapY);
		
				if (!_cellmapMiddleChunk[x, y] && _obstacleLayers.ChasmMap[coords.x, coords.y] && !IsNeighbourChasm(x,y))
				{
					InstantiateOnNeighbors(x, y, objectToInstantiate);
				}
			}
		}
	}
	private bool IsNeighbourChasm(int x, int y)
	{
		int intPosX = (int)transform.position.x;
		int intPosZ = (int)transform.position.z;

		int[] xOffset = { -1, 1, 0, 0, -1, 1, -1, 1 };
		int[] yOffset = { 0, 0, -1, 1, -1, -1, 1, 1 };

		for (int i = 0; i < 8; i++)
		{
			int newX = x + xOffset[i];
			int newY = y + yOffset[i];

			int mapX = intPosX + newX;
			int mapY = intPosZ + newY;
			Vector2Int coords = _obstacleLayers.GetMapCoords(mapX, mapY);
			if (!_obstacleLayers.ChasmMap[coords.x, coords.y])
			{
				return true;
			}
		}
		return false;
	}
	private void InstantiateOnNeighbors(int x, int y, GameObject objectToInstantiate)
	{
		// Define the relative positions of all 8 neighboring cells
		int[] xOffset = { -1, 1, 0, 0};
		int[] yOffset = { 0, 0, -1, 1};

		for (int i = 0; i < 4; i++)
		{
			int newX = x + xOffset[i];
			int newY = y + yOffset[i];

			if ((newX >= 0 && newX < _width + _border * 2 && newY >= 0 && newY < _height + _border * 2 && _cellmapWithBorders[newX, newY]))
			{
				Vector3 targetPosition = new Vector3(transform.position.x + newX, transform.position.y + _yOffset + _wallHeight, transform.position.z + newY);
				if (!Physics.Raycast(targetPosition, Vector3.down, out _, 0.1f))
				{
					GameObject go = Instantiate(objectToInstantiate, targetPosition, Quaternion.identity, transform);
					_tempObjects.Add(go);
				}
			}
		}
	}

	private (bool[,], int) ExploreEmptyArea(int startX, int startY)
	{
		int intPosX = (int)transform.position.x;
		int intPosZ = (int)transform.position.z;

		Queue<Vector2Int> queue = new Queue<Vector2Int>();
		int totalX = 0;
		int totalY = 0;
		int cellCount = 0;

		int mapWidthWithBorders = _width + 2 * _border;
		int mapHeightWithBorders = _height + 2 * _border;

		bool[,] visited = new bool[mapWidthWithBorders, mapHeightWithBorders];
		bool[,] newMap = new bool[mapWidthWithBorders, mapHeightWithBorders];

		for (int x = 0; x < mapWidthWithBorders; x++)
		{
			for (int y = 0; y < mapHeightWithBorders; y++)
			{
				newMap[x, y] = true;
			}
		}

		queue.Enqueue(new Vector2Int(startX, startY));

		List<Vector2Int> emptyAreaCells = new List<Vector2Int>();

		while (queue.Count > 0)
		{
			Vector2Int current = queue.Dequeue();
			int x = current.x;
			int y = current.y;

			int mapX = intPosX + x;
			int mapY = intPosZ + y;
			Vector2Int coords = _obstacleLayers.GetMapCoords(mapX, mapY);

			if (x < 0 || x >= mapWidthWithBorders || y < 0 || y >= mapHeightWithBorders || visited[x, y] || _cellmapWithBorders[x, y] || !_obstacleLayers.ChasmMap[coords.x, coords.y])
				continue;

			if (!IsChasm(x,y))
			{
				newMap[x, y] = _cellmapWithBorders[x, y];
			}
			else
			{
				newMap[x, y] = true;
			}
			visited[x, y] = true;
			totalX += x;
			totalY += y;
			cellCount++;

			emptyAreaCells.Add(new Vector2Int(x, y));

			// Add neighboring cells to the queue
			queue.Enqueue(new Vector2Int(x + 1, y));
			queue.Enqueue(new Vector2Int(x - 1, y));
			queue.Enqueue(new Vector2Int(x, y + 1));
			queue.Enqueue(new Vector2Int(x, y - 1));
		}

		for (int i = 0; i < visited.GetLength(0); i++)
		{
			for (int j = 0; j < visited.GetLength(1); j++)
			{
				if (visited[i, j])
					_visited[i, j] = true;
			}
		}

		// Calculate the center of the empty area
		int centerX = cellCount > 0 ? totalX / cellCount : startX;
		int centerZ = cellCount > 0 ? totalY / cellCount : startY;

		Vector2Int nearestEmptyCell = emptyAreaCells
		.OrderBy(cell => Vector2Int.Distance(new Vector2Int(centerX, centerZ), cell))
		.FirstOrDefault();

		_middleCell = nearestEmptyCell;
		_cellmapMiddleChunk = newMap;
		return (newMap, cellCount);
	}

#endregion

	#region Merging
	private void Merge()
	{
		if (_isMerged)
			return;

		Vector3 oldPos = transform.position;
		transform.position = Vector3.zero;

		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		List<List<MeshFilter>> meshBatches = new();
		Dictionary<Material, List<MeshFilter>> materialMeshGroups = new();

		foreach (MeshFilter meshFilter in meshFilters)
		{
			MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();

			if (!materialMeshGroups.TryGetValue(meshRenderer.sharedMaterial, out List<MeshFilter> group))
			{
				group = new List<MeshFilter>();
				materialMeshGroups[meshRenderer.sharedMaterial] = group;
			}

			group.Add(meshFilter);
		}

		ClearChunk();

		foreach (var materialGroup in materialMeshGroups)
		{
			List<MeshFilter> meshesWithMaterial = materialGroup.Value;
			List<MeshFilter> currentBatch = new();
			int currentBatchVertices = 0;

			foreach (MeshFilter meshFilter in meshesWithMaterial)
			{
				Mesh mesh = meshFilter.sharedMesh;
				int meshVertices = mesh.vertices.Length;

				if (currentBatchVertices + meshVertices > s_MaxVerticesPerBatch)
				{
					meshBatches.Add(currentBatch);
					currentBatch = new List<MeshFilter>();
					currentBatchVertices = 0;
				}

				currentBatch.Add(meshFilter);
				currentBatchVertices += meshVertices;
			}

			if (currentBatch.Count > 0)
			{
				meshBatches.Add(currentBatch);
			}
		}

		foreach (var batch in meshBatches)
		{
			MergeMeshBatch(batch, transform);
		}

		transform.position = oldPos;
		_isMerged = true;
	}
	private void MergeMeshBatch(List<MeshFilter> batch, Transform parentTransform)
	{
		if (batch.Count == 0)
		{
			return;
		}

		CombineInstance[] combineInstances = new CombineInstance[batch.Count];

		Vector3 groupCenter = Vector3.zero;
		foreach (MeshFilter meshFilter in batch)
		{
			groupCenter += meshFilter.transform.position;
		}
		groupCenter /= batch.Count;

		for (int i = 0; i < batch.Count; i++)
		{
			combineInstances[i].mesh = batch[i].sharedMesh;
			combineInstances[i].transform = batch[i].transform.localToWorldMatrix;
		}

		GameObject mergedMeshObject = new("MergedMesh_Batch");
		mergedMeshObject.transform.parent = parentTransform;

		Vector3 offset = groupCenter - mergedMeshObject.transform.localPosition;
		mergedMeshObject.transform.localPosition = groupCenter;

		MeshFilter mergedMeshFilter = mergedMeshObject.AddComponent<MeshFilter>();
		MeshRenderer mergedMeshRenderer = mergedMeshObject.AddComponent<MeshRenderer>();

		Mesh mergedMesh = new();
		mergedMesh.CombineMeshes(combineInstances, true, true);
		mergedMeshFilter.sharedMesh = mergedMesh;

		if (batch[0].GetComponent<MeshRenderer>().sharedMaterial != null)
		{
			mergedMeshRenderer.material = batch[0].GetComponent<MeshRenderer>().sharedMaterial;
		}

		mergedMeshObject.transform.localPosition -= offset;
		mergedMeshObject.AddComponent<Tile>();
		mergedMeshObject.AddComponent<MeshCollider>();
		mergedMeshObject.isStatic = true;
		mergedMeshObject.layer = 3;
	}
	#endregion

	public void MiningStart()
	{
		InstantiateOnWallNeighbors(_miningStart);
	}

	public void MiningEnd()
	{
		//InstantiateOnWallNeighbors(_miningEnd);
		StartCoroutine(MiningEndOnGenerated());
	}

	private IEnumerator MiningEndOnGenerated()
	{
		while (!_isGenerated)
			yield return null;
		InstantiateOnWallNeighbors(_miningEnd);
	}

	public void Detach()
	{
		StartCoroutine(ChunkDetaching());
	}
	public void Solidify()
	{
		StartCoroutine(ChunkMerging());
	}
	public void Reload()
	{
		StartCoroutine(ChunkReloading());
	}
	public void Redraw()
	{
		if (!_isMerged)
		{
			DrawCells();
		}
	}
	public void Clean()
	{
		foreach (var temp in _tempObjects)
		{
			Destroy(temp);
		}
		_tempObjects.Clear();

		StartCoroutine(ChunkCleaning());
	}

	private IEnumerator ChunkCleaning()
	{
		yield return null;
		Redraw();
	}

	private IEnumerator ChunkDetaching()
	{
		if (_isMerged)
		{
			ClearChunk();
			yield return null;
			if (_generate)
			{
				Draw();
			}
		}
		else
		{
			Redraw();
		}
	}
	private IEnumerator ChunkMerging()
	{
		UpdateBorderMap();
		DrawCells();
		yield return null;
		if (!_isMerged)
		{
			Merge();
		}
	}
	private IEnumerator ChunkReloading()
	{
		UpdateBorderMap();
		yield return null;
		if (_generate)
		{
			ClearChunk();
			Draw();
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(ChunkGenerator))]
public class ChunkGenerator_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		ChunkGenerator script = (ChunkGenerator)target;

		if (GUILayout.Button("Generate"))
			script.Instantiate(script.gameObject.transform.position, 0);
		if (GUILayout.Button("Merge"))
			script.Solidify();
		if (GUILayout.Button("Detach"))
			script.Detach();
		if (GUILayout.Button("Redraw"))
			script.Redraw();
		if(GUILayout.Button("Reload"))
			script.Reload();
	}
}
#endif