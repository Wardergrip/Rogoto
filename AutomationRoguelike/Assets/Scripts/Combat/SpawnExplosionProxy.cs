using Trivial;
using UnityEngine;

public class SpawnExplosionProxy : MonoBehaviour
{
    [SerializeField] private MonoExplosion _explosion;
    [SerializeField] private bool _useRandomRot = true;
    private bool _isQuitting = false;

    public void Spawn(ProjectileHitData hitData)
    {
        if (_isQuitting) return;
        MonoExplosion explosion = Instantiate(_explosion, hitData.Projectile.ProjectileTransform.position, _useRandomRot ? new Quaternion().MakeRandom(LockAxis.XZ) : Quaternion.LookRotation(transform.forward));
        explosion.Projectile = hitData.Projectile;
        explosion.Damage = hitData.Projectile.Damage;
    }

    public void Spawn(Transform otherTransform)
    {
        if (_isQuitting) return;
        Instantiate(_explosion,otherTransform.transform.position, otherTransform.rotation);
    }

    private void OnApplicationQuit()
    {
        _isQuitting = true;
    }
}
