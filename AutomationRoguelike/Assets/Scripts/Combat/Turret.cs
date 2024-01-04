using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Trivial;

public enum TargetPriority
{
	First, Last, Closest, Weakest, Strongest
}

public class Turret : MonoBehaviour
{
	private class TurretTargetCommunicator : MonoBehaviour
	{
		public Turret Turret { get; set; }

		private void OnTriggerEnter(Collider other)
		{
			if (!other.CompareTag("Enemy")) return;
			Turret.Targets.Add(other.transform);

			other.GetComponent<Enemy>().OnDeath.AddListener(() =>
			{		
					Turret.Targets.Remove(other.transform);
			});
		}

		private void OnTriggerExit(Collider other)
		{
			if (!other.CompareTag("Enemy")) return;
			Turret.Targets.Remove(other.transform);
            other.GetComponent<Enemy>().OnDeath.RemoveListener(() =>
            {
                Turret.Targets.Remove(other.transform);
            });
        }
	}

	public static List<Turret> s_Turrets { get; private set; } = new();
	public static event Action<Turret> OnTurretSpawned;
	public static event Action<Turret> OnTurretDestroyed;

	[Header("Fundamental")]
	[SerializeField] private PlayerStructure _playerStructure;
	public PlayerStructure PlayerStructure { get => _playerStructure; }
	[SerializeField] private SpawnObjectsFromImage _spawnObjectsFromImage;
	public SpawnObjectsFromImage SpawnObjectsFromImage { get => _spawnObjectsFromImage; }

	[Header("Projectile")]
	[SerializeField] private Projectile _projectilePrefab;
	public Projectile ProjectilePrefab { get => _projectilePrefab; set => _projectilePrefab = value; }
	[SerializeField] private bool _cycleSockets = false;
	private int _currentSocket = 0;
	[SerializeField] private List<Transform> _projectileOutputSockets = new();
	[SerializeField] private TargetPriority _targetPriority;
	public TargetPriority TargetPriority { get => _targetPriority; set => _targetPriority = value; }
	[SerializeField] private LookAt _lookAt;
	private bool _lookingAtTarget = false;

	[Header("Stats")]
	[SerializeField] private Damage _damage;
	public Damage Damage { get => _damage; }
	[SerializeField] private TimeBetweenAttacks _timeBetweenAttacks;
	public TimeBetweenAttacks TimeBetweenAttacks { get => _timeBetweenAttacks; }
	[SerializeField] private float _projectileSpeed;
	public float ProjectileSpeed { get => _projectileSpeed; }
	public int Tier { get; private set; } = 1;

	[Header("Events")]
	public UnityEvent OnTierChanged;
	public UnityEvent<Transform> OnUpgradeTwo;
	public UnityEvent<Transform> OnUpgradeThree;
	public event Action<Turret> OnAwakeTurret;
	public event Action<Turret> OnSwapTarget;
	public event Action<Turret> OnProjectileFired;
	public UnityEvent<Transform> OnProjectileFire;

	public event Action<Turret> OnTurretSpawnedInRange;
	public event Action<Turret> OnTurretDestroyedInRange;
	public event Action<ProjectileHitData> OnProjectileHit;
	public event Action<ProjectileHitData> OnProjectileKill;
	public event Action<SpawnObjectsFromImage> OnTurrretRangeSpawned;

	public UnityEvent<Stat> OnStatBuffed = new();
	public event Action<Stat> OnStatVisualNeedsRefresh;

	[Header("Targets")]
	[SerializeField] private bool _shootAtAllSetTargets = true;
	public bool ShootAtAllSetTargets { get => _shootAtAllSetTargets; set => _shootAtAllSetTargets = value; }
	[Tooltip("If this is empty, it will not be used. Order is matched with output sockets")]
	[SerializeField] private List<Transform> _setTargets;
	public List<Transform> SetTargets { get => _setTargets; }
	public Transform CurrentTarget { get; private set; }
	private Enemy _currentEnemyTarget;
	private List<Transform> _targets = new();

	public List<Transform> Targets { get => _targets; }
	private Coroutine _attackingCoroutine;
	private Transform _lastShotEnemy;


	private void Awake()
	{
		s_Turrets.Add(this);

		Debug.Assert(!_spawnObjectsFromImage.ParseOnStart, $"Make sure the linked SpawnObjectsFromImage parse on start is disabled");
		OnAwakeTurret?.Invoke(this);
		_spawnObjectsFromImage.OnObjectsSpawned.AddListener(RangeObjectsSpawned);
		_spawnObjectsFromImage.ParseAndSpawn();

		for (int i = 0; i < _playerStructure.Stats.Count; ++i) 
		{
			_playerStructure.Stats[i].OnBuff += StatBuff;
			_playerStructure.Stats[i].OnVisualNeedsRefresh += StatVisualNeedsRefresh;
		}

		//OnProjectileHit += (ProjectileHitData phd) => { Debug.Log($"Projectile hit: {phd.HediffHandler.gameObject.name}. Health left: {phd.HediffHandler.Health.HealthAmount}"); };
		OnTurretSpawned?.Invoke(this);
	}

	private void StatBuff(Stat obj)
	{
		OnStatBuffed?.Invoke(obj);
	}

	private void StatVisualNeedsRefresh(Stat obj)
	{
		OnStatVisualNeedsRefresh?.Invoke(obj);
	}

	private void Start()
	{
		SetTargetBoxesVisual(false);
		PerformOnTurretsThatThisIsInRangeOf(x => x.OnTurretSpawnedInRange?.Invoke(this));
	}

	private void OnDestroy()
	{
		PerformOnTurretsThatThisIsInRangeOf(x => x.OnTurretDestroyedInRange?.Invoke(this));

		for (int i = 0; i < _playerStructure.Stats.Count; ++i)
		{
			_playerStructure.Stats[i].OnBuff -= StatBuff;
			_playerStructure.Stats[i].OnVisualNeedsRefresh -= StatVisualNeedsRefresh;
		}

		s_Turrets.Remove(this);
		OnTurretDestroyed?.Invoke(this);
	}

	/// <summary>
	/// Checks for ranges this turret is in, performs action on that turret.
	/// </summary>
	/// <param name="action">Param is turret that this turret is in range of.</param>
	private void PerformOnTurretsThatThisIsInRangeOf(Action<Turret> action)
	{
		BoxCollider coll = GetComponent<BoxCollider>();
		Collider[] overlappingColls = Physics.OverlapBox(coll.bounds.center, coll.bounds.extents);
		List<Turret> notifiedTurrets = new();
		foreach (Collider overlapping in overlappingColls)
		{
			if (overlapping.TryGetComponent(out TurretTargetCommunicator turretTargetCommunicator))
			{
				if (turretTargetCommunicator.Turret == this) continue;
				if (notifiedTurrets.Find(x => x == turretTargetCommunicator.Turret) == null)
				{
					action(turretTargetCommunicator.Turret);
					notifiedTurrets.Add(turretTargetCommunicator.Turret);
				}
			}
		}
	}

	#region EnemySpawnKillEvents
	private void OnEnable()
	{
		GameSystem.OnEnemyStartSpawning += GameSystem_OnEnemyStartSpawning;
		GameSystem.OnEnemyAllKilled += GameSystem_OnEnemyAllKilled;
	}

	private void OnDisable()
	{
		GameSystem.OnEnemyStartSpawning -= GameSystem_OnEnemyStartSpawning;
		GameSystem.OnEnemyAllKilled -= GameSystem_OnEnemyAllKilled;
	}

	private void GameSystem_OnEnemyStartSpawning()
	{
		_attackingCoroutine = StartCoroutine(AttackingCoroutine());
	}

	private void GameSystem_OnEnemyAllKilled()
	{
		if (_attackingCoroutine != null)
		{
			StopCoroutine(_attackingCoroutine);
		}
		_targets.Clear();
		ResetTurretRotation();
	}
	#endregion

	private void ResetTurretRotation()
	{
		if (_lookAt == null) return;
		foreach (GameObject obj in _lookAt.ObjectsToRotate)
		{
			Vector3 eulerAngles = obj.transform.rotation.eulerAngles;
			eulerAngles.x = 0.0f;
			obj.transform.rotation = Quaternion.Euler(eulerAngles);
		}
	}

	private IEnumerator AttackingCoroutine()
	{
		if (_timeBetweenAttacks == null)
		{
			yield break;
		}
		while (true)
		{
			yield return new WaitUntil(() => _targets.Count > 0);
			ShootAtEnemy();
			yield return new WaitForSeconds(_timeBetweenAttacks.FinalValue);
			if (_lookAt != null) _lookAt.ObjectToLookAt = null; // If target leaves range it will still follow it.
		}
	}

	private void RangeObjectsSpawned()
	{
		foreach (GameObject spawnedObj in _spawnObjectsFromImage.SpawnedObjects)
		{
			spawnedObj.AddComponent<TurretTargetCommunicator>().Turret = this;
		}
		OnTurrretRangeSpawned?.Invoke(_spawnObjectsFromImage);
	}

	#region Shooting code
	private Transform GetTarget()
	{
		var validTargets = _targets.Where(t => t != null && t.GetComponent<Enemy>() != null).ToList();
		if (validTargets.Count == 0) return null;
		_targets = validTargets;
		switch (_targetPriority)
		{
			case TargetPriority.First:
				return validTargets.FindClosest(Base.Position);
			case TargetPriority.Last:
				return validTargets.SortByDistance(Base.Position).Last();
			case TargetPriority.Closest:
				return validTargets.FindClosest(transform.position);
			case TargetPriority.Weakest:
				return validTargets.OrderBy(t => t.GetComponent<Enemy>().Health.HealthAmount).FirstOrDefault();
			case TargetPriority.Strongest:
				return validTargets.OrderByDescending(t => t.GetComponent<Enemy>().Health.HealthAmount).FirstOrDefault();
			default:
				Debug.LogWarning($"Target priority not implemented yet: {_targetPriority}");
				break;
		}
		return null;
	}

	private void ShootProjectile(Transform socket, Transform target, bool shouldProjectileExpectEnemy = false)
	{
		Projectile proj = Instantiate(_projectilePrefab, socket.position, socket.rotation);
		ProjectileInitData projectileInitData = new(this, target);
		if (_damage != null) projectileInitData.Damage = Mathf.RoundToInt(_damage.FinalValue);
		projectileInitData.Speed = _projectileSpeed;
		projectileInitData.DontExpectEnemy = shouldProjectileExpectEnemy;
		proj.Initialise(projectileInitData);
		proj.OnHit.AddListener((ProjectileHitData phd) => { OnProjectileHit?.Invoke(phd); });
		proj.OnKill.AddListener((ProjectileHitData phd) => { OnProjectileKill?.Invoke(phd); });
		OnProjectileFired?.Invoke(this);
		OnProjectileFire?.Invoke(socket);
	}

	private void ShootAtEnemy()
	{
		if (GameSystem.Instance.IsPaused) return;
		if (_setTargets.Count > 0)
		{
			ShootAtSpots();
			return;
		}
		Transform target = CurrentTarget;
		CurrentTarget = GetTarget();
		if (_lastShotEnemy != CurrentTarget)
		{
			OnSwapTarget?.Invoke(this);
		}
		_lastShotEnemy= CurrentTarget != null ? CurrentTarget : null;
		_currentEnemyTarget = CurrentTarget !=  null ? CurrentTarget.GetComponent<Enemy>() : null;
		if (CurrentTarget == null || _currentEnemyTarget.Socket == null)
		{
			if (_lookAt != null) _lookAt.ObjectToLookAt = null;
			return;
		}
		if (_lookAt != null)
		{
			if (!_lookAt.IsLookingAtObject)
			{
				return;
			}
		}
		

		if (_cycleSockets)
		{
			ShootProjectile(_projectileOutputSockets[CycleSocket()], _currentEnemyTarget.Socket);
		}
		else
		{
			foreach (Transform socket in _projectileOutputSockets)
			{
				ShootProjectile(socket, _currentEnemyTarget.Socket);
			}
		}
	}

	private void Update()
	{
		if (_lookAt != null)
		{
			CurrentTarget = GetTarget();
			if (_currentEnemyTarget == null)
			{
				_currentEnemyTarget = CurrentTarget != null ? CurrentTarget.GetComponent<Enemy>() : null;
			}
			_lookAt.ObjectToLookAt = _currentEnemyTarget != null ? _currentEnemyTarget.Socket.gameObject : null;
		}
	}

	private int CycleSocket()
	{
		_currentSocket = (_currentSocket + 1) % _projectileOutputSockets.Count;
		return _currentSocket;
	}

private void ShootAtSpots()
	{
		if (!ShootAtAllSetTargets)
		{
			for (int i = 0; i < _projectileOutputSockets.Count; ++i)
			{
				ShootProjectile(_projectileOutputSockets[i], _setTargets[0], true);
			}
			return;
		}
		for (int i = 0; i < _setTargets.Count; ++i)
		{
			ShootProjectile(_projectileOutputSockets[i % _projectileOutputSockets.Count], _setTargets[i], true);
		}
	}
	#endregion

	#region Target boxes visual
	public void SetTargetBoxesVisual(bool state)
	{
		foreach (GameObject spawnedObj in _spawnObjectsFromImage.SpawnedObjects)
		{
			spawnedObj.transform.GetChild(0).gameObject.SetActive(state);
		}
	}
	#endregion

	public void Upgrade()
	{
        SetTier(Mathf.Clamp(Tier + 1, 1, 3));
	}
	public void SetTier(int tier)
	{
		Tier = tier;
		foreach (Stat stat in _playerStructure.Stats)
		{
			stat.SetTier(tier);
		}
		OnTierChanged.Invoke();

		if (tier == 2)
		{
			OnUpgradeTwo.Invoke(transform);
		}
		else if (tier == 3)
		{
			OnUpgradeThree.Invoke(transform);
		}
	}

	public TargetPriority CyclePriorityForward()
	{
		int currentPriorityIndex = (int)_targetPriority;
		currentPriorityIndex = (currentPriorityIndex + 1) % Enum.GetNames(typeof(TargetPriority)).Length;
		_targetPriority = (TargetPriority)currentPriorityIndex;
		return _targetPriority;
	}

	public TargetPriority CyclePriorityBackward()
	{
		int currentPriorityIndex = (int)_targetPriority;
		currentPriorityIndex = (currentPriorityIndex - 1 + Enum.GetNames(typeof(TargetPriority)).Length) % Enum.GetNames(typeof(TargetPriority)).Length;
		_targetPriority = (TargetPriority)currentPriorityIndex;
		return _targetPriority;
	}

}
