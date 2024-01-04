using System.Linq;
using UnityEditor;
using UnityEngine;

public class Tile : MonoBehaviour
{
	[SerializeField] private LayerMask _mask;
	[SerializeField] private int gridSize = 1;
	private readonly bool[] _neighborStates = new bool[8];

	[SerializeField] private Mesh _noneMesh;

	[SerializeField] private Mesh _singleMesh;

	[SerializeField] private Mesh _doubleMesh;
	[SerializeField] private Mesh _cornerMesh;
	[SerializeField] private Mesh _cornerChippedMesh;

	[SerializeField] private Mesh _tripleMesh;
	[SerializeField] private Mesh _tripleMeshLeftChipped;
	[SerializeField] private Mesh _tripleMeshRightChipped;
	[SerializeField] private Mesh _tripleMeshDoubleChipped;

	[SerializeField] private Mesh _quadMesh;
	[SerializeField] private Mesh _quadMeshSingleChipped;
	[SerializeField] private Mesh _quadMeshDoubleOpposed;
	[SerializeField] private Mesh _quadMeshDoubleConnected;
	[SerializeField] private Mesh _quadMeshTripleChipped;
	[SerializeField] private Mesh _quadMeshQuadChipped;
	

	public void CheckNeighborStates()
	{
		Vector3 currentPosition = transform.position;

		Vector3[] neighborPositions = new Vector3[]
		{
			new Vector3(-gridSize, 0, gridSize), // Top Left
            new Vector3(0, 0, gridSize),  // Top Middle
            new Vector3(gridSize, 0, gridSize),  // Top Right

            new Vector3(-gridSize, 0, 0),  // Middle Left
            new Vector3(gridSize, 0, 0),   // Middle Right

            new Vector3(-gridSize, 0, -gridSize),  // Bottom Left
            new Vector3(0, 0, -gridSize),   // Bottom Middle
            new Vector3(gridSize, 0, -gridSize)    // Bottom Right
        };

		for (int i = 0; i < neighborPositions.Length; i++)
		{
			Vector3 neighborPosition = currentPosition + neighborPositions[i];
			bool neighborExists = Physics.Raycast(neighborPosition + new Vector3(0, gridSize, 0), Vector3.down, out _, gridSize, _mask);
			//bool hasDesiredComponent = neighborExists && hitInfo.collider.gameObject.GetComponent<Tile>() != null;
			//Debug.DrawRay(neighborPosition + new Vector3(0, gridSize, 0), Vector3.down, Color.blue, 10f);

			_neighborStates[i] = neighborExists;
		}
	}
	public void Draw()
	{
		MeshFilter mf = transform.GetComponentInChildren<MeshFilter>();
		if (mf == null)
		{
			DestroyImmediate(gameObject);
			return;
		}

		CheckNeighborStates();

		bool[] wallStates = new bool[] { _neighborStates[1], _neighborStates[3], _neighborStates[4], _neighborStates[6] };
		int wallCount = wallStates.Count(condition => condition);

		bool[] cornerStates = new bool[] { _neighborStates[2], _neighborStates[7], _neighborStates[5], _neighborStates[0] };
		int cornerCount = cornerStates.Count(condition => condition);

		int rotationWall = 0;

		// Not that great, it works and is readable
		switch (wallCount) 
		{
			case 0:
				mf.mesh = _noneMesh;
				break;
			case 1:
				mf.mesh = _singleMesh;
				rotationWall = wallStates[0] ? 0 : (wallStates[2] ? 1 : (wallStates[3] ? 2 : 3));
				break;
			case 2:
				// Opposing
				if ((wallStates[0] && wallStates[3]) || (wallStates[1] && wallStates[2]))
				{
					mf.mesh = _doubleMesh;
					rotationWall = (wallStates[0] && wallStates[3]) ? 0 : 1;
				}
				//Corner
				else
				{
					rotationWall = wallStates[0] && wallStates[2] ? 0 : (wallStates[2] && wallStates[3] ? 1 : (wallStates[3] && wallStates[1] ? 2 : (wallStates[1] && wallStates[0] ? 3 : rotationWall)));
					if (!cornerStates[rotationWall])
						mf.mesh = _cornerChippedMesh;
					else
						mf.mesh = _cornerMesh;
				}
				break;
			case 3:
				rotationWall = !wallStates[0] ? 0 : (!wallStates[2] ? 1 : (!wallStates[3] ? 2 : 3));

				switch (rotationWall)
				{
					case 0:
						mf.mesh = (!cornerStates[2] && !cornerStates[1]) ? _tripleMeshDoubleChipped
								  : (!cornerStates[2]) ? _tripleMeshLeftChipped
								  : (!cornerStates[1]) ? _tripleMeshRightChipped
								  : _tripleMesh;
						break;
					case 1:
						mf.mesh = (!cornerStates[3] && !cornerStates[2]) ? _tripleMeshDoubleChipped
								  : (!cornerStates[3]) ? _tripleMeshLeftChipped
								  : (!cornerStates[2]) ? _tripleMeshRightChipped
								  : _tripleMesh;
						break;
					case 2:
						mf.mesh = (!cornerStates[0] && !cornerStates[3]) ? _tripleMeshDoubleChipped
								  : (!cornerStates[0]) ? _tripleMeshLeftChipped
								  : (!cornerStates[3]) ? _tripleMeshRightChipped
								  : _tripleMesh;
						break;
					case 3:
						mf.mesh = (!cornerStates[1] && !cornerStates[0]) ? _tripleMeshDoubleChipped
								  : (!cornerStates[1]) ? _tripleMeshLeftChipped
								  : (!cornerStates[0]) ? _tripleMeshRightChipped
								  : _tripleMesh;
						break;
				}
				break;
			case 4:
				switch (cornerCount)
				{
					case 0:
						mf.mesh = _quadMeshQuadChipped;
						break;
					case 1:
						mf.mesh = _quadMeshTripleChipped;
						rotationWall = cornerStates[0] ? 0 : cornerStates[1] ? 1 : cornerStates[2] ? 2 : cornerStates[3] ? 3 : rotationWall;
						break;
					case 2:
						//Opposing
						if ((cornerStates[0] && cornerStates[2]) || (cornerStates[1] && cornerStates[3]))
						{
							mf.mesh = _quadMeshDoubleOpposed;
							rotationWall = (cornerStates[1] && cornerStates[3]) ? 0 : 1;
						}
						//Connected
						else
						{
							mf.mesh = _quadMeshDoubleConnected;
							rotationWall = cornerStates[3] && cornerStates[0] ? 0 : (cornerStates[0] && cornerStates[1] ? 1 : (cornerStates[1] && cornerStates[2] ? 2 : (cornerStates[2] && cornerStates[3] ? 3 : rotationWall)));
						}
						break;
					case 3:
						mf.mesh = _quadMeshSingleChipped;
						rotationWall = !cornerStates[0] ? 0 : !cornerStates[1] ? 1 : !cornerStates[2] ? 2 : !cornerStates[3] ? 3 : rotationWall;
						break;
					case 4:
						mf.mesh = _quadMesh;
						break;
				}
				break;
		}
		rotationWall *= 90;
		transform.rotation = Quaternion.Euler(0.0f, rotationWall, 0.0f);
	}
}
#if UNITY_EDITOR
[CustomEditor(typeof(Tile))]
public class Tile_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		Tile script = (Tile)target;

		if (GUILayout.Button("Check"))
			script.CheckNeighborStates();
		if (GUILayout.Button("Draw"))
			script.Draw();
	}
}
#endif