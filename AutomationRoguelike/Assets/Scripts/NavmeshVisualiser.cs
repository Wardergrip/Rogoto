using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NavmeshVisualiser : MonoBehaviour
{
	[SerializeField] private LineRenderer _mainLineRenderer;
	[SerializeField] private LineRenderer _otherLineRenderer;
	[SerializeField] private float _mainVerticalOffset = 0.5f;
	[SerializeField] private float _otherVerticalOffset = 0.6f;
	private NavMeshPath _path;

	private void Start()
	{
		_path = new();

		_mainLineRenderer.enabled = false;
		_otherLineRenderer.enabled = false;
		NavMeshRebaker.OnRebake += Calculate;
		GameSystem.OnEnemyStartSpawning += SetInActive;
		GameSystem.OnEnemyAllKilled += SetActive;
	}

	private void SetActive()
	{
		this.gameObject.SetActive(true);
	}
	private void SetInActive()
	{
		this.gameObject.SetActive(false);
	}

	public void Calculate()
    {
		StartCoroutine(CalculateCoroutine());
	}

	private IEnumerator CalculateCoroutine()
	{
		if (EnemyNest.s_CurrentTargetNest == null)
		{
			yield return new WaitUntil(() => EnemyNest.s_CurrentTargetNest != null);
		}
		transform.position = EnemyNest.s_CurrentTargetNest.transform.position;

		bool success = NavMesh.CalculatePath(EnemyNest.s_CurrentTargetNest.transform.position, Base.Position, 0xFFFFFF, _path);
		_mainLineRenderer.enabled = success;
		if (_otherLineRenderer != null) _otherLineRenderer.enabled = success;
		if (success)
		{
			CalculateLineRendPoints(_path);
		}
	}

	private void CalculateLineRendPoints(NavMeshPath path)
	{
		Vector3[] mainPoints = new Vector3[path.corners.Length];
		Vector3[] otherPoints = new Vector3[path.corners.Length];
		for (int i = 0; i < path.corners.Length; i++)
		{
			mainPoints[i] = path.corners[i];
			otherPoints[i] = path.corners[i];
			mainPoints[i].y += _mainVerticalOffset;
			otherPoints[i].y += _otherVerticalOffset;
		}
		_mainLineRenderer.positionCount = mainPoints.Length;
		_mainLineRenderer.SetPositions(mainPoints);
		if (_otherLineRenderer != null)
		{
			_otherLineRenderer.positionCount = mainPoints.Length;
			_otherLineRenderer.SetPositions(otherPoints);
		}
	}

	private void OnDestroy()
	{
		NavMeshRebaker.OnRebake -= Calculate;
        GameSystem.OnEnemyStartSpawning -= SetInActive;
		GameSystem.OnEnemyAllKilled -= SetActive;
    }
}
