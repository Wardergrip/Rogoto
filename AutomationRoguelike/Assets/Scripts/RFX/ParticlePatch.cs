using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Trivial;

[CreateAssetMenu(menuName = "ParticlePatch")]
public class ParticlePatch : ScriptableObject
{
    public GameObject[] _particlePrefabs;
    private List<GameObject> _uniqueParicles;
	[SerializeField] private float _longestEffectTime = 1.0f;

	private void Awake()
	{
		Debug.Assert(_particlePrefabs.Length > 0,"ParticlePrefabs is empty!");
		if (_particlePrefabs != null && _particlePrefabs.Length > 0)
		{
			_uniqueParicles = _particlePrefabs.ToList();
		}
	}

	public void PlayAttached(Transform transform)
	{
		if (!transform.gameObject.activeInHierarchy)
		{
			Debug.LogWarning($"[ParticlePatch] {name} is played on an inactive transform and is refusing to playattached.");
			return;
		}
		GameObject obj = Instantiate(ReturnRandomPrefab(), transform);
		DestroyTimer destroyTimer = obj.AddComponent<DestroyTimer>();
		destroyTimer.Time = _longestEffectTime;
		destroyTimer.StartTimer();
	}

	public void PlayTrailing(Transform transform)
	{
		GameObject trailing = SpawnTrailing(transform);
		PlayAttached(trailing.transform);
	}

	private GameObject ReturnRandomPrefab()
    {
		if (_particlePrefabs.Length == 1)
		{
			return _particlePrefabs[0];
		}
		Debug.Assert(_particlePrefabs.Length != 0, $"ParticlePatch ({name}) is empty!");
        if (_uniqueParicles.Count <= 0) 
        {
			_uniqueParicles = _particlePrefabs.ToList();
		}
        int randomIndex = Random.Range(0, _uniqueParicles.Count); 
        GameObject randomPrefab = _uniqueParicles[randomIndex]; 
		_uniqueParicles.RemoveAt(randomIndex); 

        return randomPrefab; //returns this element
    }

	private GameObject SpawnTrailing(Transform transform)
	{
		GameObject obj = new("[ParticlePatch] Trailing Particles");
		obj.transform.SetPositionAndRotation(transform.position, transform.rotation);
		DestroyTimer destroyTimer = obj.AddComponent<DestroyTimer>();
		destroyTimer.Time = _longestEffectTime;
		destroyTimer.StartTimer();
		return obj;
	}
}