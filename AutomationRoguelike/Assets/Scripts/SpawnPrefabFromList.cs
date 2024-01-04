using UnityEngine;

public class SpawnPrefabFromList : MonoBehaviour
{
	[SerializeField] private GameObject[] _objects;

	void Awake()
	{
		spawnObject();
	}

	public void spawnObject()
	{
		GameObject randomObject = _objects[Random.Range(0, _objects.Length)];
		Instantiate(randomObject, transform.position, Quaternion.identity, transform);
	}
}
