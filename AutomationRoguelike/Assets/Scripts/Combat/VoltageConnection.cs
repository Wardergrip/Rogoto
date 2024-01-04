using UnityEngine;
using UnityEngine.Events;

public class VoltageConnection : MonoBehaviour
{
	[SerializeField] private ParticleSystem _particleSystem;
	[SerializeField] private LineRenderer _lineRenderer;
	[SerializeField] private float _enemyHeight = 1.5f;

	public UnityEvent<Transform> OnTriggered;

	private void Awake()
	{
		if ( _particleSystem != null )
			_particleSystem.transform.position = new(_particleSystem.transform.position.x, _enemyHeight, _particleSystem.transform.position.z);
	}

	public void Connect(Vector3 first, Vector3 second)
	{
		first.y = _enemyHeight;
		second.y = _enemyHeight;
		Vector3[] positions = { first, second };
		_lineRenderer.SetPositions(positions);
		transform.position = first;
		OnTriggered.Invoke(transform);
	}
}
