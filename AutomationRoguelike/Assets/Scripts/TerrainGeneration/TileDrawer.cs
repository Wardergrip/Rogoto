using UnityEngine;

public class TileDrawer : MonoBehaviour
{
	[SerializeField] private Vector3 areaSize = new Vector3(3, 3, 3);

	private void Awake()
	{
		DrawTilesInArea();
		Destroy(gameObject, 3);
	}

	void DrawTilesInArea()
	{
		Collider[] colliders = Physics.OverlapBox(transform.position, areaSize / 2);

		foreach (Collider col in colliders)
		{
			Tile tileComponent = col.GetComponentInParent<Tile>();

			if (tileComponent != null)
			{
				tileComponent.Draw();
			}
		}
	}
}
