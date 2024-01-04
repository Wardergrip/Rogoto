using System.Collections;
using System.Collections.Generic;
using Trivial;
using UnityEngine;

public class LaserTurretProjectile : MonoBehaviour
{
	private bool _isInit = false;
    [SerializeField] private Projectile _projectile;
	[SerializeField] private Transform _laserEndPoint;
	[SerializeField] private OnColliderTriggerEvents _laserEndPointTriggerEvents;
	[SerializeField] private LineRenderer _lineRenderer;
	[SerializeField] private float _laserSpeed = 0.4f;
	private Turret _turret;
	private List<Transform> _setTargets;

	private readonly List<HediffHandler> _hitHediffs = new(10);

	private void Awake()
	{
		_projectile.OnInitialise.AddListener(OnProjInit);
	}

	private void OnDestroy()
	{
		_projectile.OnInitialise.RemoveListener(OnProjInit);
		if (_laserEndPointTriggerEvents) _laserEndPointTriggerEvents.OnTriggerEnterEvent.RemoveListener(LaserEndPointOnTriggerEnter);
	}

	private void OnProjInit()
	{
		Debug.Assert(!_isInit,"Initialise of projectile is invoked multiple times.");
		_isInit = true;
		
		_turret = _projectile.Shooter;
		Debug.Assert(_turret != null);
		_setTargets = _turret.SetTargets;
		if (_laserEndPointTriggerEvents == null) _laserEndPointTriggerEvents = _laserEndPoint.GetComponent<OnColliderTriggerEvents>();
		_laserEndPointTriggerEvents.OnTriggerEnterEvent.AddListener(LaserEndPointOnTriggerEnter);

		StartCoroutine(MoveLaserCoroutine());
		StartCoroutine(UpdateLineRendererCoroutine());
	}

	private void LaserEndPointOnTriggerEnter(Collider other)
	{
		if (!other.TryGetComponent(out HediffHandler handler)) return;
		if (_hitHediffs.Contains(handler)) return;

		handler.Health.Damage(_projectile.Damage);
		_hitHediffs.Add(handler);

		ProjectileHitData projectileHitData = new(handler, _projectile);
		if (handler.Health.HealthAmount <= 0)
		{
			_projectile.OnKill?.Invoke(projectileHitData);
		}
		else
		{
			_projectile.OnHit?.Invoke(projectileHitData);
		}
	}

	private void ReachedDestination()
	{
		Destroy(gameObject);
	}

	private IEnumerator UpdateLineRendererCoroutine()
	{
		_lineRenderer.positionCount = 2;
		_lineRenderer.SetPosition(0,transform.position);
		while (true) 
		{
			_lineRenderer.SetPosition(1,_laserEndPoint.position);
			yield return null;
		}
	}

	private IEnumerator MoveLaserCoroutine()
	{
		float t = 0;
        while (t < 1.0f)
        {
			_setTargets.Lerp(t, _laserEndPoint);
			t += Time.deltaTime * _laserSpeed;
			yield return null;
        }
		ReachedDestination();
	}
}
