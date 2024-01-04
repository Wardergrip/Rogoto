using System;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class ObstacleLayers : MonoBehaviour
{
	[SerializeField] private int _width;
	[SerializeField] private int _height;
	public int Width => _width;
	public int Height => _height;

	[SerializeField] private CellularAutomata _chasmGenerator;
	[SerializeField] private CellularAutomata _pillarGenerator;

	public bool[,] ChasmMap { get; private set; }
	public bool[,] PillarMap { get; private set; }

	void Awake()
	{
		Generate();
	}

	public void Generate()
	{
		ChasmMap = _chasmGenerator.Generate(_width, _height);
		PillarMap = _pillarGenerator.Generate(_width, _height);

		ApplyFunctionToGrid(_width, _height, (x, y) =>
		{
			if (!ChasmMap[x, y])
			{
				PillarMap[x, y] = true;
			}
		});

		ApplyFunctionToGrid(_width, _height, (x, y) =>
		{
			int[] dx = { -1, 1, 0, 0, -1, -1, 1, 1 };
			int[] dy = { 0, 0, -1, 1, -1, 1, -1, 1 };

			// Check neighboring cells (left, right, up, down)
			bool hasNeighborPillar = false;

			for (int i = 0; i < 4; i++)
			{
				int newX = x + dx[i];
				int newY = y + dy[i];

				if (newX >= 0 && newX < _width && newY >= 0 && newY < _height && !PillarMap[newX, newY])
				{
					hasNeighborPillar = true;
					break;
				}
			}
			if (hasNeighborPillar)
			{
				ChasmMap[x, y] = true;

				for (int i = 0; i < 8; i++)
				{
					int newX = x + dx[i];
					int newY = y + dy[i];

					if (newX >= 0 && newX < _width && newY >= 0 && newY < _height)
					{
						ChasmMap[newX, newY] = true;
					}
				}
			}
		});

		PillarMap = _pillarGenerator.SimulateStep(PillarMap);
	}

	public Vector2Int GetMapCoords(int x, int y)
	{
		x = (x + _width) % _width;
		y = (y + _height) % _height;

		return new Vector2Int(x, y);
	}

	public static void ApplyFunctionToGrid(int width, int height, Action<int, int> action)
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				action(x, y);
			}
		}
	}

	//	private void OnDrawGizmosSelected()
	//	{
	//		if (ChasmMap == null || PillarMap == null)
	//			return;
	//	
	//		Vector3 objPosition = gameObject.transform.position;
	//		Vector3 cubeSize = Vector3.one * .5f;
	//	
	//		Gizmos.color = Color.blue;
	//	
	//		for (int x = 0; x < ChasmMap.GetLength(0); x++)
	//		{
	//			for (int y = 0; y < ChasmMap.GetLength(1); y++)
	//			{
	//				if (!ChasmMap[x, y])
	//				{
	//					Gizmos.DrawCube(new Vector3(objPosition.x + x, -1, objPosition.z + y), cubeSize);
	//				}
	//			}
	//		}
	//	
	//		Gizmos.color = Color.red;
	//	
	//		for (int x = 0; x < PillarMap.GetLength(0); x++)
	//		{
	//			for (int y = 0; y < PillarMap.GetLength(1); y++)
	//			{
	//				if (!PillarMap[x, y])
	//				{
	//					Gizmos.DrawCube(new Vector3(objPosition.x + x, 1, objPosition.z + y), cubeSize);
	//				}
	//			}
	//		}
	//	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(ObstacleLayers))]
public class ObstacleLayers_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		ObstacleLayers script = (ObstacleLayers)target;

		if (GUILayout.Button("Generate"))
			script.Generate();
	}
}
#endif
