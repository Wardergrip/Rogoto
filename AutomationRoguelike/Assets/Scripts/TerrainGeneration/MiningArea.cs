using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class MiningArea : MonoBehaviour
{
	[Header("Settings")]
	[SerializeField] private int _size;
	[SerializeField] private float _wallHeight;
	[SerializeField] private float _yOffset = .5f;
	[SerializeField] private LayerMask _layerMask;

	[Header("Tile Prefabs")]
	[SerializeField] private GameObject _wall;
	[SerializeField] private GameObject _ground;
	[SerializeField] private GameObject _pillar;

	[SerializeField] private GameObject[] _chasmObjects;

	[Header("Generators")]
	[SerializeField] private ObstacleLayers _obstacleLayers;

	private bool[,] _mask;
	private bool[,] _cellMap;

	private bool _isMerged = false;

	public int Size { get => _size; set => _size = value; }

	private static readonly int s_MaxVerticesPerBatch = 65535; //Max amounts of verts in unity


	public Vector3 MiddlePosition()
	{
		return transform.position + new Vector3(_size *.5f, 0, _size * .5f) ;
	}

	public void Instantiate(Vector3 pos)
	{
		_mask = new bool[_size, _size];
		_cellMap = new bool[_size, _size];
		UpdateMask();
		Draw(transform.position);
	}
	private void UpdateMask()
	{
		for (int x = 0; x < _size; x++)
		{
			for (int y = 0; y < _size; y++)
			{
				const float raycastDistance = 10f;
				Vector3 offset = new Vector3(transform.position.x + x, transform.position.y + raycastDistance*.5f, transform.position.z + y);

				Ray ray = new Ray(offset + Vector3.up * raycastDistance, Vector3.down);

				RaycastHit hitInfo;
				bool hasTile = Physics.Raycast(ray, out hitInfo, raycastDistance * 2, _layerMask);

				if (hasTile && hitInfo.collider != null)
				{
					_mask[x, y] = true;
				}
				else
				{
					_mask[x, y] = false;
				}
			}
		}
	}

	private void SaveCellmap()
	{
		for (int x = 0; x < _size; x++)
		{
			for (int y = 0; y < _size; y++)
			{
				if (!_mask[x, y])
				{
					Vector3 offset = new(transform.position.x + x, transform.position.y + _yOffset, transform.position.z + y);

					const float checkSize = 0.2f;
					Collider[] colliders = Physics.OverlapSphere(offset, checkSize);
					if (colliders.Length > 0)
					{
						bool hasTile = true;
						foreach (Collider collider in colliders)
						{
							if (collider.GetComponentInParent<Tile>() != null)
							{
								hasTile = false;
								break;
							}
						}
						_cellMap[x, y] = hasTile;
					}
					else
					{
						_cellMap[x, y] = true;
					}
				}
			}
		}
	}

	private void Draw(Vector3 pos)
	{
		int intPosX = (int)pos.x;
		int intPosZ = (int)pos.z;

		for (int x = 0; x < _size; x++)
		{
			for (int y = 0; y < _size; y++)
			{
				int mapX = intPosX + x;
				int mapY = intPosZ + y;
				Vector2Int coords = _obstacleLayers.GetMapCoords(mapX, mapY);

				Vector3 offset = new(transform.position.x + x, transform.position.y + _yOffset, transform.position.z + y);
				Vector3 floorPos = offset + new Vector3(0, -1, 0);
				Vector3 wallPos = offset;
				Vector3 pillarPos = offset + new Vector3(0, _wallHeight, 0);

				if (_mask[x, y] == true)
					continue;

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

				if (_cellMap[x, y] == true)
					continue;

				// Walls
				if (!_obstacleLayers.PillarMap[coords.x, coords.y])
				{
					InstantiateAndName(_wall, wallPos);
				}
				else if(_obstacleLayers.ChasmMap[coords.x, coords.y])
				{
					InstantiateAndName(_wall, wallPos);
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
	private void DrawCells()
	{
		var tiles = GetComponentsInChildren<Tile>();
		foreach (var tile in tiles)
		{
			tile.Draw();
		}
	}
	private void InstantiateAndName(GameObject prefab, Vector3 position)
	{
		var go = Instantiate(prefab, position, Quaternion.identity, transform);
		go.name = position.ToString();
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

		foreach (Transform child in transform)
		{
			Destroy(child.gameObject);
		}

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

	public void Detach()
	{
		StartCoroutine(AreaDetaching());
	}
	public void Solidify()
	{
		StartCoroutine(AreaMerging());
	}
	public void Redraw()
	{
		StartCoroutine(AreaReloading());
	}
	public void Clean()
	{
		StartCoroutine(AreaCleaning());
	}

	private IEnumerator AreaDetaching()
	{
		if (_isMerged)
		{
			foreach (Transform child in transform)
			{
				Destroy(child.gameObject);
			}
			yield return null;
			Draw(transform.position);
		}
		else
		{
			DrawCells();
		}
	}
	private IEnumerator AreaMerging()
	{
		DrawCells();
		yield return null;
		Merge();
	}

	private IEnumerator AreaCleaning()
	{
		yield return null;
		DrawCells();
	}

	private IEnumerator AreaReloading()
	{
		yield return new WaitForSeconds(.1f);
		SaveCellmap();
		foreach (Transform child in transform)
		{
			Destroy(child.gameObject);
		}
		yield return new WaitForEndOfFrame();
		UpdateMask();
		Draw(transform.position);
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(MiningArea))]
public class MiningArea_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		MiningArea script = (MiningArea)target;

		if (GUILayout.Button("Generate"))
			script.Instantiate(script.gameObject.transform.position);
		if (GUILayout.Button("Redraw"))
			script.Redraw();
	}
}
#endif