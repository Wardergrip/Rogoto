using System;
using UnityEngine;
using UnityEngine.Events;

public class ProjectileHitData
{
    /// <summary>
    /// Perform null check! Is null on the OnDestroyEvent
    /// </summary>
    public HediffHandler HediffHandler { get; private set; }
    public Projectile Projectile { get; private set; } 
    public ProjectileHitData(HediffHandler hh, Projectile projectile)
    {
        HediffHandler = hh;
        Projectile = projectile;
    }
}

public class ProjectileInitData
{
    public Transform Target { get; set; }
    public int Damage { get; set; }
    public float Speed { get; set; }
    public bool DontExpectEnemy { get; set; }
	public Turret Shooter { get; set; }
	public ProjectileInitData(Turret shooter, Transform target, int damage = 1, float speed = 1, bool dontExpectEnemy = false)
    {
        Target = target;
        Damage = damage;
        Speed = speed;
        DontExpectEnemy = dontExpectEnemy;
        Shooter = shooter;
    }
}

public class Projectile : MonoBehaviour
{
    public bool DontExpectEnemy { get; set; } = false;
    public Transform Target { get; set; }
    private Vector3 _previousTargetPos;
    public int Damage { get; set; }
	[Obsolete] public int Pierce { get; set; }
    public float Speed { get; set; } = 1.0f;
	public Turret Shooter { get; set; }

	[Tooltip("Only used if projectile shouldn't expect enemy or if target is null")]
    [SerializeField] private float _selfDestructionDistance = 0.5f;

    [Header("Assign this if you want to use animation for movement")]
    [SerializeField] private Animator _animator;

    [Header("Events")]
    public UnityEvent OnInitialise;
    public UnityEvent<ProjectileHitData> OnHit;
    public UnityEvent<ProjectileHitData> OnKill;
    public UnityEvent<ProjectileHitData> OnDestroyed;

    /// <summary>
    /// Returns transform of the actual projectile even if an animator is applied.
    /// </summary>
    public Transform ProjectileTransform { get => _animator == null ? transform : _animator.transform; }

    public void Initialise(ProjectileInitData projectileInitData)
    {
        Target = projectileInitData.Target.root.transform;
        Damage = projectileInitData.Damage;
        Speed = projectileInitData.Speed;
        DontExpectEnemy = projectileInitData.DontExpectEnemy;
        Shooter = projectileInitData.Shooter;
		OnInitialise?.Invoke();
	}
   
    private void OnDestroy()
    {
        OnDestroyed?.Invoke(new(null, this));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform != Target) return;
        if (!other.TryGetComponent(out HediffHandler hh)) return;

        hh.Health.Damage(Damage);

		ProjectileHitData phd = new(hh, this);

        if (hh.Health.HealthAmount <= 0)
        {
			OnKill?.Invoke(phd);
		}
        else
        {
			OnHit?.Invoke(phd);
		}

        Destroy(gameObject);
    }

    private void Start()
    {
        if (_animator == null) return;
        _animator.SetFloat("AnimSpeed", Speed);
    }

    private void Update()
    {
        if (_animator != null)
        {
            return;
        }
        if (Target != null)
        {
            _previousTargetPos = Target.position;
        }

        Vector3 dir = _previousTargetPos - transform.position;
        dir.y = 0;
        transform.position += Speed * Time.deltaTime * GameSystem.Instance.GameSpeed * dir.normalized;
        bool isInSelfDestructRange = dir.sqrMagnitude <= _selfDestructionDistance;
        if (((Target == null) || DontExpectEnemy) && isInSelfDestructRange)
        {
            Destroy(gameObject);
        }
    }

    public void ForceDestroy()
    {
        Destroy(gameObject);
    }
}
