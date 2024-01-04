using UnityEngine;

public class RandomSpawnRotation : MonoBehaviour
{
	[SerializeField] private bool _lockToCardinalDirections = false;

	void Start()
	{
		float randomYRotation = 0;
		if (_lockToCardinalDirections)
		{
			randomYRotation = Random.Range(0, 3)*90;
		}
		else
		{
			randomYRotation = Random.Range(0f, 360f);
		}
		
		transform.rotation = Quaternion.Euler(0f, randomYRotation, 0f);
	}
}
