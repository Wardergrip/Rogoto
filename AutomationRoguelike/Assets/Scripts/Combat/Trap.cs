using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Trivial;

[Obsolete]
public interface ITrapAttack
{
    /// <summary>
    /// Attack is run everytime.
    /// </summary>
    /// <param name="trap"></param>
    public void Attack(Trap trap);
    /// <summary>
    /// In case the trap wants to wait until a certain condition, you can return a predicate. By default, it returns null which means the trap does not wait for the predicate.
    /// </summary>
    /// <returns></returns>
    public virtual Func<bool> GetWaitUntilPred() { return null; }
}
[Obsolete]
public class DebugTrapAttack : ITrapAttack
{
    public void Attack(Trap trap)
    {
        Debug.Log($"DebugTrapAttack: {trap.gameObject.name}");
    }
}
[Obsolete]
public class Trap : MonoBehaviour
{
    public static List<Trap> s_Traps { get; private set; } = new();
	public static event Action<Trap> OnTrapSpawned;
	public static event Action<Trap> OnTrapDestroyed;

	[Header("Fundamental")]
    [SerializeField] private Collider _collider;
    [SerializeField] private PlayerStructure _playerStructure;
    public PlayerStructure PlayerStructure { get => _playerStructure; }
    public ITrapAttack TrapAttack { get; set; } = new DebugTrapAttack();
    [Header("Stats")]
    [SerializeField] private Damage _damage;
    public Damage Damage { get => _damage; }
    [SerializeField] private TimeBetweenAttacks _timeBetweenAttacks;
    public TimeBetweenAttacks TimeBetweenAttacks { get => _timeBetweenAttacks; }
    public List<GameObject> ObjectsInsideTrap { get; private set; } = new();
    [Header("Events")]
    public UnityEvent OnTrapEnter;
    public UnityEvent OnTrapExit;
    public UnityEvent OnTierChanged;

    public int Tier { get; private set; } = 1;

    private Coroutine _attackingCoroutine;

    private void Awake()
    {
        s_Traps.Add(this);

		Debug.Assert(_collider.isTrigger);
        TryInitUnitialisedComponents();

        OnTrapSpawned?.Invoke(this);
    }

	private void OnDestroy()
	{
        s_Traps.Remove(this);
        OnTrapDestroyed?.Invoke(this);
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
    }
    #endregion

    private IEnumerator AttackingCoroutine()
    {
        while (true)
        {
            yield return new WaitUntil(() => ObjectsInsideTrap.Count > 0);
            TrapAttack.Attack(this);
            Func<bool> pred = TrapAttack.GetWaitUntilPred();
            if (pred != null)
            {
                yield return new WaitUntil(pred);
            }
            yield return new WaitForSeconds(_timeBetweenAttacks.FinalValue);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;
        ObjectsInsideTrap.Add(other.gameObject);
        OnTrapEnter?.Invoke();
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;
        ObjectsInsideTrap.Remove(other.gameObject);
        OnTrapExit?.Invoke();
    }

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
        OnTierChanged?.Invoke();
    }

    private void TryInitUnitialisedComponents()
    {
        _damage = this.FindOrAddComponent(_damage);
        _timeBetweenAttacks = this.FindOrAddComponent(_timeBetweenAttacks);
    }
}
