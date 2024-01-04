using System.Linq;
using UnityEngine;

public class MakeTargetBoxesSetTargets : MonoBehaviour
{
    [SerializeField] private SpawnObjectsFromImage _spawnObjectsFromImage;
    [SerializeField] private Turret _turret;

    public void Execute()
    {
		//https://learn.microsoft.com/en-us/dotnet/api/system.comparison-1?view=net-8.0
		// return Less than 0       => x is less than y.
		// return 0                 => x equals y.
		// return Greater than 0    => x is greater than y.
		System.Comparison<GameObject> comparison = 
            (GameObject x, GameObject y) => 
            {
                float distX = (x.transform.position - _turret.transform.position).sqrMagnitude;
                float distY = (y.transform.position - _turret.transform.position).sqrMagnitude;
                return (int)(distX - distY); 
            };


        _turret.SetTargets.Clear();
        _spawnObjectsFromImage.SpawnedObjects.Sort(comparison);
        _turret.SetTargets.AddRange(_spawnObjectsFromImage.SpawnedObjects.Select(obj => obj.transform));
    }
}
