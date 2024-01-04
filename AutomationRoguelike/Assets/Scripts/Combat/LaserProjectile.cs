using System.Collections;
using System.Collections.Generic;
using Trivial;
using UnityEngine;
using UnityEngine.Events;

public class LaserProjectile : MonoBehaviour
{
    [SerializeField] private bool _nestKiller;
    public Vector3 TargetPos;
    [SerializeField] private float _speed;
    public int Damage;
    public UnityEvent OnExplode;
    [SerializeField] private float _explosionRadius;
    [SerializeField] private bool _hasAcceleration;
    private float _targetSpeed = 0;
    private float _accelerationSpeed = 1.5f;
    private float _timer;
    private void Start()
    {
        _targetSpeed = _speed;
    }
    private void Update()
    {
        _timer += Time.deltaTime * _accelerationSpeed;
        if (_hasAcceleration)
        {
            _speed = Mathf.Lerp(0, _targetSpeed, _timer);
        }
        MoveToTarget();
    }
    
    private void MoveToTarget()
    {
        if (TargetPos == null)
            return;
        transform.LookAt(TargetPos);
        transform.position += _speed * Time.deltaTime * transform.forward;
        if (Vector3.Distance(transform.position, TargetPos)<=0.2f+(_speed*0.01))
        {
            ReachedTarget();
        }
    }
    private void ReachedTarget()
    {
        OnExplode.Invoke();
        if (_nestKiller)
        {
            EnemyNest.s_CurrentTargetNest.DamageNest();
        }
        else
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, _explosionRadius);
            DamageEnemies(cols);
        }
        Destroy(this.gameObject);
        return;
    }
    private void DamageEnemies(Collider[] colliders)
    {
        foreach (Collider collider in colliders)
        {
            HediffHandler enemy = collider.GetComponent<HediffHandler>();
            if (enemy != null)
            {
                enemy.Health.Damage(Damage) ;
            }
        }
    }
}
