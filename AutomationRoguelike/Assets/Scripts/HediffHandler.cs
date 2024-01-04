using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public abstract class Hediff
{
    /// <summary>
    /// Every Tick will get lowered by HediffHandler.s_TickTime
    /// </summary>
    public float TimeLeft { get; set; }
    /// <summary>
    /// Used to differentiate hediffs from eachother. If id is the same reapply will be called when it is already present.
    /// </summary>
    public string Identifier { get; private set; }
    public bool IsStackable{ get; private set; }
    public int Stacks { get; private set; } = 1;
    public Hediff(string identifier, float timeLeft, bool isStackable)
    {
        TimeLeft = timeLeft;
        Identifier = identifier;
        IsStackable = isStackable;
    }

   

    public void RootApply() => Apply();
    public void RootTick() => Tick();
    public void RootReapply(Hediff hediff) 
    { 
        Reapply(hediff);
        ++Stacks;
    } 
    public void RootRemove() => Remove();

    /// <summary>
    /// Runs first time said effect is added.
    /// </summary>
    public virtual void Apply() { }
    /// <summary>
    /// Runs every tick interval.
    /// </summary>
    public virtual void Tick() { }
    /// <summary>
    /// Runs instead of Apply. You can implement if it stacks or refreshes or both. Hediff you get is the newest one trying to be applied. 
    /// </summary>
    /// <param name="hediff">The newest one trying to be applied</param>
    public virtual void Reapply(Hediff hediff) { }
    /// <summary>
    /// Runs once it gets removed. It will run once even though reapply has been called multiple times.
    /// </summary>
    public virtual void Remove() { }
}

[RequireComponent(typeof(Health))]
public class HediffHandler : MonoBehaviour
{
    [SerializeField] private Health _health;
    [SerializeField] private Enemy _enemy;
    public Health Health { get { return _health; } }
    public Enemy Enemy { get { return _enemy; } }
    [Header("Events")]
    public UnityEvent<int> OnDamageTaken;
    public UnityEvent<int, Health.DamageType> OnDamageTakenType;
    public UnityEvent<int> OnHealReceived;
    public UnityEvent OnDeath;
    public UnityEvent<int> OnArmorReduced;

    public static float s_TickTime { get { return 0.5f; } }
    private readonly Dictionary<string, Hediff> _hediffs = new();
    private readonly List<Hediff> _hediffList = new();
    public UnityEvent<Hediff> OnHediffUpdated;
    public UnityEvent<Hediff> OnHediffRemoved;



    private void Start()
    {
        if (_health == null) _health = GetComponent<Health>();

        _health.OnDied.AddListener(() => { OnDeath?.Invoke(); });
        _health.OnTookDamage.AddListener((int val) => { OnDamageTaken?.Invoke(val); });
		_health.OnTookDamageType.AddListener((int val, Health.DamageType damageType) => 
        { 
            OnDamageTakenType?.Invoke(val, damageType);
			OnDamageTaken?.Invoke(val);
		});
		_health.OnGainedHealth.AddListener((int val) => { OnHealReceived?.Invoke(val); });
        _health.OnArmorReduced.AddListener((int val) => { OnArmorReduced?.Invoke(val); });
        StartCoroutine(HediffTick());
    }

    private IEnumerator HediffTick()
    {
        List<Hediff> hediffsToBeRemoved = new();
        while (true)
        {
            yield return new WaitUntil(() => { return _hediffList.Count > 0; });
            yield return new WaitForSeconds(s_TickTime);
            hediffsToBeRemoved.Clear();
            foreach (Hediff hediff in _hediffList)
            {
                hediff.RootTick();
                hediff.TimeLeft -= s_TickTime;
                if (hediff.TimeLeft <= 0)
                {
                    hediffsToBeRemoved.Add(hediff);
                }
            }
            hediffsToBeRemoved.ForEach(h => RemoveHediff(h));
        }
    }

    public void AddHediff(Hediff hediff)
    {
        if (_hediffs.ContainsKey(hediff.Identifier))
        {
            _hediffs[hediff.Identifier].RootReapply(hediff);
            OnHediffUpdated.Invoke(_hediffs[hediff.Identifier]);
            return;
        }
        _hediffs.Add(hediff.Identifier, hediff);
        _hediffList.Add(hediff);
        hediff.RootApply();
        OnHediffUpdated.Invoke(hediff);
    }
    public bool RemoveHediff(string identifier)
    {
        if (_hediffs.TryGetValue(identifier,out Hediff hediff))
        {
            return RemoveHediff(hediff);
        }
        return false;
    }
    private bool RemoveHediff(Hediff hediff)
    {
        hediff.RootRemove();
        _hediffs.Remove(hediff.Identifier);
        OnHediffRemoved.Invoke(hediff);
        return _hediffList.Remove(hediff);
    }
}
