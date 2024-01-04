using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyRingProjectile : MonoBehaviour
{
  
    [SerializeField] private float _sliceSpeed = 1;
    private Projectile _parentProj;
    private static int _id = 0;
    private List<HediffHandler> _hitHediffs = new();

    private void Start()
    {
        _id++;
        _parentProj = transform.parent.GetComponent<Projectile>();
        StartCoroutine(Slice());
    }
    private IEnumerator Slice()
    {
        float scale = 0;
        while(scale < 1)
        {
            scale += Time.deltaTime * _sliceSpeed;
            transform.localScale = new Vector3(scale,1,scale);
            yield return null;
        }
        Destroy(transform.parent.gameObject);
        yield break;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out HediffHandler handler))
        {
            if (!_hitHediffs.Contains(handler))
            {
                handler.Health.Damage(_parentProj.Damage);
				if (handler.Health.HealthAmount > 0)
					_parentProj.OnHit.Invoke(new ProjectileHitData(handler, _parentProj));
				else
					_parentProj.OnKill.Invoke(new ProjectileHitData(handler, _parentProj));
				_hitHediffs.Add(handler);
			}
        }
    }
}
