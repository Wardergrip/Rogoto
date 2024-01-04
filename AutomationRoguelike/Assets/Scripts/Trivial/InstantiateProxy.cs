using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateProxy : MonoBehaviour
{
    [SerializeField] GameObject _objectToSpawn;

    public void Spawn()
    {
        Instantiate(_objectToSpawn, transform.position, transform.rotation);
    }

	public void Spawn(Transform trans)
	{
		Instantiate(_objectToSpawn, trans.position, trans.rotation);
	}
}
