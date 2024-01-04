using System;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEngine;

public class NavMeshRebaker : MonoSingleton<NavMeshRebaker>
{
    public static event Action OnRebake;

    [SerializeField] private NavMeshSurface _surface;

    public void Rebake()
    {
        _surface.BuildNavMesh();
        OnRebake?.Invoke();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NavMeshRebaker))]
public class NavMeshRebaker_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		NavMeshRebaker script = (NavMeshRebaker)target;

		if (GUILayout.Button("Rebake"))
			script.Rebake();
	}
}
#endif