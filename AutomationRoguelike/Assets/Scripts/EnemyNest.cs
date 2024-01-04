using System.Collections;
using System.Collections.Generic;
using Trivial;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class EnemySwarmPreset
{
	private int _colums;
	private int _maxRows;
	private float _spacingColums;
	private float _spacingRows;
	private Enemy _enemy;
	private int _minimumEnemies;
	private Enemy[] _customEnemySwarm;
	private int _levelToAdd;

	public int MinimumEnemies { get => _minimumEnemies; }
	public Enemy Enemy { get => _enemy; }
	public float SpacingRows { get => _spacingRows;  }
	public float SpacingColums { get => _spacingColums;  }
	public int MaxRows { get => _maxRows;  }
	public int Colums { get => _colums;  }
    public Enemy[] CustomEnemySwarm { get => _customEnemySwarm;}
    public int LevelToAdd { get => _levelToAdd; }

    public EnemySwarmPreset(int colums, int maxRows, float spacingColums, float spacingRows,
		Enemy enemy, int minimumEnemies, int levelToAdd)
	{
		_colums = colums;
		_maxRows = maxRows;
		_spacingColums = spacingColums;
		_spacingRows = spacingRows;
		_enemy = enemy;
		_minimumEnemies = minimumEnemies;
		_levelToAdd = levelToAdd;
	}

    public EnemySwarmPreset(int colums, int maxRows, float spacingColums, float spacingRows,
		int minimumEnemies, Enemy[] customEnemySwarm,int levelToAdd)
    {
        _colums = colums;
        _maxRows = maxRows;
        _spacingColums = spacingColums;
        _spacingRows = spacingRows;
        _minimumEnemies = minimumEnemies;
        _customEnemySwarm = customEnemySwarm;
		_levelToAdd = levelToAdd;
    }

    public int GetMaximumSpots()
	{
		return Colums * MaxRows;
	}
}
public class EnemySwarm 
{
	private List<Enemy> _loadedEnemies = new();
	private EnemySwarmPreset _preset;
	private bool _customSwarm = false;
	private float _averageSpeed;
    public float LowestSpeed { get => _averageSpeed; }
    public bool CustomSwarm { get => _customSwarm; }

    public int LoadSwarm(int nestBudget, EnemyNest nest, EnemySwarmPreset preset)
	{
		_preset = preset;
        if (_preset.CustomEnemySwarm!=null)
        {
			float totalSpeed = 0;
			int totalEnemyCount = 0;
            foreach (Enemy enemy in _preset.CustomEnemySwarm)
            {
				totalSpeed += enemy.BaseMovementSpeed;
				_loadedEnemies.Add(enemy);
				nestBudget -= enemy.Cost;
				++totalEnemyCount;
            }
			_averageSpeed = totalSpeed/totalEnemyCount;
			_customSwarm = true;
			return nestBudget;
        }
		int maxEnemiesCanBuy = Mathf.Min(_preset.GetMaximumSpots(), nestBudget / _preset.Enemy.Cost);
		if (maxEnemiesCanBuy <= _preset.MinimumEnemies)
		{
			maxEnemiesCanBuy = _preset.MinimumEnemies;
		}
        for (int i = 0; i < Random.Range(_preset.MinimumEnemies, maxEnemiesCanBuy + 1); i++)
        {
			_loadedEnemies.Add(_preset.Enemy);
			nestBudget -= _preset.Enemy.Cost;
        }
		return nestBudget;
	}
	public IEnumerator SpawnSwarm(EnemyNest nest)
	{
		int i = 0;
		foreach(Enemy enemy in _loadedEnemies)
		{
			Vector3 positionOnGrid= new Vector3( i %_preset.MaxRows ,0, i / _preset.Colums);
			Vector3 positionFinalOffset = new Vector3(positionOnGrid.x * _preset.SpacingColums,0, positionOnGrid.z * _preset.SpacingRows);
			nest.SpawnSingularEnemy(enemy, nest.transform.position+new Vector3(-1,0,-1) +positionFinalOffset,Quaternion.identity,this);
			yield return new WaitForSeconds(0.1f);
			i++;
		}
	}
}
public class EnemyNest : MonoBehaviour
{
    public static EnemyNest s_CurrentTargetNest;
	public static Vector3? s_PreviousNestPos = null;
	private List<EnemySwarmPreset> _swarmPresets = new();
	public static List<EnemyNest> ActiveNests = new();
	[SerializeField] List<Enemy> _enemyPreFabs = new();
	[SerializeField] protected int _level = 0;
	public int Level { get => _level; set => _level = value; }
	private int _waveBudget;
	[SerializeField] int _waveOneBudget;
	[SerializeField] int _budgetIncreasePerWave;
	[SerializeField] private float _timeBetweenSwarms = 2f;
	private readonly Queue<EnemySwarm> _loadedSwarms = new();
	[SerializeField] protected Health _health;
	[SerializeField] private HealthBar _healthBarPreFab;
	protected HealthBar _healthBar;
	[SerializeField] protected Color _healthBarColorOverride = Color.white;
	[SerializeField] protected float _secondsUntilDamageEvent = 2;
	[SerializeField] private Animator _nestAnimator;

	public Queue<EnemySwarm> LoadedSwarms { get => _loadedSwarms; }
	bool _isQuitting = false;

	public UnityEvent OnSpawnWave;

	protected virtual void Awake()
	{
		ActiveNests.Add(this);
        _level = GameSystem.Instance.WaveCounter;
    }
    public void Start()
	{
		CameraController.EnqueueCinematic(new CinematicCommand(transform, 2.0f));
		GameSystem.OnBuildPhaseStarted += ChooseThisNest;
		s_CurrentTargetNest = this;
	}
	public void ChooseThisNest()
	{
		s_CurrentTargetNest = this;
        GameSystem.OnBuildPhaseStarted -= ChooseThisNest;
    }
    public void Initialize()
	{
		GameSystem.OnEnemyStartSpawning += StartSpawning;
		GameSystem.OnEnemyAllKilled += LoadSwarms;
		_healthBar = Instantiate(_healthBarPreFab, transform.position, Quaternion.identity);
		_healthBar.SetUpHealthBar(transform, _health, _healthBarColorOverride, _level);
		SetSwarmPresets();
		LoadSwarms();
	}
	private void SetSwarmPresets()
	{
		_swarmPresets.Add(new EnemySwarmPreset(4, 4, 0.5f, 0.5f, _enemyPreFabs[3], 10, 4));
		_swarmPresets.Add(new EnemySwarmPreset(3, 3, 0.6f, 0.6f, _enemyPreFabs[0], 3,1));
		_swarmPresets.Add(new EnemySwarmPreset(2, 2, 1f, 1f, _enemyPreFabs[2], 2,10));
		_swarmPresets.Add(new EnemySwarmPreset(1, 1, 2f, 2f, _enemyPreFabs[1], 1,7));
		_swarmPresets.Add(new EnemySwarmPreset(3, 3, 1, 1, 9, new Enemy[]
		{
			_enemyPreFabs[1],
			_enemyPreFabs[0],
			_enemyPreFabs[0],
			_enemyPreFabs[2],
			_enemyPreFabs[2],
			_enemyPreFabs[2],
			_enemyPreFabs[0],
			_enemyPreFabs[0],
			_enemyPreFabs[1],
		},13));
	}

	public EnemySwarmPreset RandomSwarmPreset(List<EnemySwarmPreset> list)
	{
		List<EnemySwarmPreset> newList = new();
        foreach (EnemySwarmPreset preset in list)
        {
			if (preset.LevelToAdd <= _level)
				newList.Add(preset);
        }
		return newList[Random.Range(0, newList.Count)];
	}
	private void LoadSwarms()
	{
		_waveBudget = _waveOneBudget+(_budgetIncreasePerWave*GameSystem.Instance.WaveCounter-1);
		while (_waveBudget >= 0)
		{
			EnemySwarm swarm = new();
			_waveBudget = swarm.LoadSwarm(_waveBudget, this, RandomSwarmPreset(_swarmPresets));
			LoadedSwarms.Enqueue(swarm);
		}
	}
	private void StartSpawning()
	{
		StartCoroutine(SpawnSwarms());
	}
	private IEnumerator SpawnSwarms()
	{
		while (LoadedSwarms.Count > 0)
		{
			OnSpawnWave.Invoke();
			StartCoroutine(LoadedSwarms.Peek().SpawnSwarm(this));
			LoadedSwarms.Dequeue();
			if (LoadedSwarms.Count <= 0)
			{
				StopAllCoroutines();
			}
			yield return new WaitForSeconds(_timeBetweenSwarms);
		}
	}
	public void SpawnSingularEnemy(Enemy enemy, Vector3 position, Quaternion rotation, EnemySwarm swarm)
	{
		
	   Enemy spawnedEnemy= Instantiate(enemy, position, rotation);
		spawnedEnemy.Level = _level;
		if (swarm.CustomSwarm)
			spawnedEnemy.BaseMovementSpeed = swarm.LowestSpeed;
		spawnedEnemy.SetUpEnemy();
	}
	public virtual void DamageNest()
	{
		// GameSystem.Instance.NestDamaged();
		
		_health.Damage(1);
		if (_health.HealthAmount <= 0)
			Death();
		else
		{
			StartCoroutine(WaitUntilDamageEvent(_secondsUntilDamageEvent));
		}
	}

	protected IEnumerator WaitUntilDamageEvent(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		GameSystem.Instance.NestDamaged();
	}

	private void OnApplicationQuit()
	{
		_isQuitting = true;
	}

	protected virtual void Death()
	{
		_nestAnimator.Play("Die");
		DestroyTimer timer = gameObject.AddComponent<DestroyTimer>();
		timer.Time = _secondsUntilDamageEvent + .2f;
		timer.StartTimer();

		if (!_isQuitting && _healthBar)
		{
			Destroy(_healthBar.gameObject);
		}

		StartCoroutine(WaitUntilDamageEvent(_secondsUntilDamageEvent));
	}

	protected virtual void OnDestroy()
	{
		GameSystem.OnEnemyStartSpawning -= StartSpawning;
		GameSystem.OnEnemyAllKilled -= LoadSwarms;
		GameSystem.OnBuildPhaseStarted -= ChooseThisNest;
		ActiveNests.Remove(this);

		if (s_CurrentTargetNest == this)
		{
			s_PreviousNestPos = transform.position;
		}

		if (!_isQuitting && _healthBar)
		{
			Destroy(_healthBar.gameObject);
		}
	}
}