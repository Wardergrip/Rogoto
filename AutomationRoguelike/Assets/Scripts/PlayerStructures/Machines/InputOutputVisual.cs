using UnityEngine;

public class InputOutputVisual : MonoBehaviour
{
	[SerializeField] private Renderer _renderer;

	[SerializeField] private Material _valid;
	[SerializeField] private Material _inValid;
	[SerializeField] private Material _neutral;

	[SerializeField] private LayerMask _mask;

	void Start()
	{
		UpdateVisual(true, true);
	}

	public void UpdateVisual(bool validPosition, bool placed)
	{
		if (placed) 
		{
			_renderer.material = _neutral;
			return;
		}
		if (validPosition)
		{
			_renderer.material = _valid;
			if (IsInsideCollider())
			{
				_renderer.material = _inValid;
			}
		}
		else
		{
			_renderer.material = _inValid;
		}
	}

	private bool IsInsideCollider()
	{

		Vector3 center = new(transform.position.x, .5f, transform.position.z);

		Debug.DrawLine(center + Vector3.up * .1f, center, Color.white, 10f);

		Collider[] colliders = Physics.OverlapSphere(center, 0.1f, _mask);
		foreach (Collider collider in colliders)
		{
			if (collider != GetComponentInParent<Collider>())
			{
				return true;
			}
		}
		return false;
	}
}
