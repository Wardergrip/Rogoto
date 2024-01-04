using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
	[SerializeField] private int _damage;
	[SerializeField] private float _baseMovementSpeed;
	private float _movementSpeed;
	[SerializeField] private NavMeshAgent _navAgent;
	[SerializeField] private HealthBar _healthBarBase;
	private HealthBar _healthBar;
	[SerializeField] private Color _healthBarColorOverride = Color.white;
	[SerializeField] private Health _health;
	public static List<Enemy> LiveEnemies = new();
	[SerializeField] private int _cost;
	private int _level;
	[SerializeField] private Transform _socket;

	public int Damage { get => _damage; }
	public Health Health { get => _health; }
	public int Level { get => _level; set => _level = value; }
	public int Cost { get => _cost; }
	public float BaseMovementSpeed { get => _baseMovementSpeed; set => _baseMovementSpeed = value; }
	public float MovementSpeed { get => _movementSpeed; }
	public Transform Socket { get => _socket; }

	public UnityEvent<Transform> OnReachStructure;
	public UnityEvent OnDeath;

	private void Awake()
	{
		LiveEnemies.Add(this);
	}

	public void UpdateMovementSpeed()
	{
		if (_navAgent == null) return;
		_navAgent.speed = GameSystem.Instance.IsPaused ? _movementSpeed : 0;
		_navAgent.speed *= GameSystem.Instance.GameSpeed;
	}
	
	public void SlowMovement(float percentage)
	{
		_movementSpeed *= percentage;
		if (_navAgent) _navAgent.speed = _movementSpeed; // is null on destroy sometimes
	}
	
	public void SetUpEnemy()
	{
		_navAgent.SetDestination(Base.Position);
		_movementSpeed = BaseMovementSpeed;
		_navAgent.speed = _movementSpeed;
		_health.ScaleHealthByLevel(_level);
		_health.OnDied.AddListener(Death);
		_healthBar = Instantiate(_healthBarBase, transform.position, Quaternion.identity);
		_healthBar.SetUpHealthBar(transform, _health, _healthBarColorOverride, _level);
		_healthBar.DebuffShower.Handler = GetComponent<HediffHandler>();
		_healthBar.DebuffShower.SetUp();
		_healthBar.gameObject.SetActive(false);
	}
	public void ReachDestroyablePlayerStructure()
	{
		OnReachStructure?.Invoke(transform);
		_health.InstaKill();
	}
	
	private void Death()
	{
		OnDeath.Invoke();
		LiveEnemies.Remove(this);
		GameSystem.Instance.CheckEnemiesRemaining();
		Destroy(_healthBar.gameObject);
		Destroy(gameObject);
	}

	public void SetHealthbarVisibility(bool visible)
	{
		_healthBar.gameObject.SetActive(visible);
	}
	private void OnDestroy()
	{
		LiveEnemies.Remove(this);
	}
}
