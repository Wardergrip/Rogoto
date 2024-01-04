using UnityEngine;

public class Miner : Machine
{
    protected override int CalculateOutputResource()
    {
		Vector3 pos = new Vector3 (transform.position.x, transform.position.y+.5f, transform.position.z);
		Ray ray = new Ray(pos, Vector3.down);
		RaycastHit hitInfo;
		Debug.DrawRay(ray.origin, ray.direction * .5f, Color.red, 10f);
		if (Physics.Raycast(ray, out hitInfo, .5f))
		{
			ResourceSpot resourceSpot = hitInfo.collider.gameObject.GetComponent<ResourceSpot>();

			if (resourceSpot != null)
			{
				resourceSpot.Occupied(true);
				return resourceSpot.Level;
			}
		}
		return 0;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		Vector3 pos = new Vector3(transform.position.x, transform.position.y + .5f, transform.position.z);
		Ray ray = new Ray(pos, Vector3.down);
		RaycastHit hitInfo;
		Debug.DrawRay(ray.origin, ray.direction * .5f, Color.red, 10f);
		if (Physics.Raycast(ray, out hitInfo, .5f))
		{
			ResourceSpot resourceSpot = hitInfo.collider.gameObject.GetComponent<ResourceSpot>();

			if (resourceSpot != null)
			{
				resourceSpot.Occupied(false);
			}
		}
	}
}
