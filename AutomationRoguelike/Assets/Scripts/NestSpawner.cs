using UnityEngine;

public class NestSpawner: MonoBehaviour
{
	[SerializeField] private GameObject _nest;

	public void Initialize(int level)
	{
		//level = Mathf.Clamp(level, -2, 2);

		if (_nest != null)
		{
			var nest = Instantiate(_nest, new(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity, transform);
			EnemyNest enemyNest = nest.GetComponent<EnemyNest>();
			enemyNest.Level += level;
			if (enemyNest.Level < 1)
			{
				enemyNest.Level = 1;
			}
			enemyNest.Initialize();
		}
	}
}
