using System.Collections;
using System.Collections.Generic;
using Trivial;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LaserCannon : MonoBehaviour
{
    [SerializeField] private Damage _damage;
    [SerializeField] private Transform _cannonTransform;
    [SerializeField] private Transform _cannonShotPoint;
    [SerializeField] private LaserProjectile _mouseShootProjectile;
    [SerializeField] private LaserProjectile _nestShootProjectile;
    [SerializeField] private TimeBetweenAttacks _attackSpeed;
    [SerializeField] private GameObject _cooldownVisual;
    [SerializeField] private GameObject _cooldownBar;
    [SerializeField] private float _timeForChargeUp = 3f;
    private bool _manualControl = false;
    private float _cannonLerpRoationSpeed = 1f;
    private bool _readyToShoot = true;

    public UnityEvent<Transform> OnNestShoot;
    public UnityEvent<Transform> OnNormalShoot;
    public UnityEvent<Transform> OnChargeNestShot;

    private void Start()
    {
        GameSystem.OnEnemyAllKilled += ChargeShotAtNest;
        GameSystem.OnEnemyStartSpawning += SwitchToManualCannon;
        GameSystem.OnEnemyAllKilled += SwitchOffManualCannon;
        _cooldownVisual.SetActive(false);
    }
    private void ChargeShotAtNest()
    {
        CameraController.EnqueueCinematic(new CinematicCommand(_cannonShotPoint, _timeForChargeUp+0.03f,4f));
        StartCoroutine(ShootAtNest());
	}
    private IEnumerator ShootAtNest()
    {
        Vector3 target = new
            (
            EnemyNest.s_CurrentTargetNest.transform.position.x,
            _cannonTransform.position.y,
            EnemyNest.s_CurrentTargetNest.transform.position.z
            );
        OnChargeNestShot?.Invoke(_cannonShotPoint);
        StartCoroutine(_cannonTransform.LerpRotationToTargetPosition(target, _cannonLerpRoationSpeed));
        yield return new WaitForSeconds(_timeForChargeUp);
        Shoot(_nestShootProjectile, target);
    }
    private void SwitchOffManualCannon()
    {
        CustomCursor.Instance.SetCursorState(CursorState.Default, true, true);

        _manualControl = false;
        _cooldownVisual.SetActive(false);
    }
    private void SwitchToManualCannon()
    {
        CustomCursor.Instance.SetCursorState(CursorState.CombatReady, true, false);
        _manualControl = true;
        _cooldownVisual.SetActive(true);
    }
    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ShootAtMouse();
        }
    }
    private void ShootAtMouse()
    {
        if (!_manualControl||_readyToShoot==false)
            return;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            Vector3 target = new Vector3(hit.point.x, _cannonTransform.position.y, hit.point.z);
            StartCoroutine(_cannonTransform.LerpRotationToTargetPosition(target, _cannonLerpRoationSpeed,
                () => Shoot(_mouseShootProjectile, new Vector3(hit.point.x,0,hit.point.z))));
        }
        StartCoroutine(TimeBetweenAttacks());
    }
    private IEnumerator TimeBetweenAttacks()
    {
        _readyToShoot = false;
        _cooldownBar.transform.localScale = Vector3.one;
        CustomCursor.Instance.SetCursorState(CursorState.CombatWait, true, false);
        float cooldown = _attackSpeed.FinalValue;
        while (cooldown > 0)
        {
            cooldown -= Time.deltaTime * GameSystem.Instance.GameSpeed;
            float cooldownPercent = cooldown / _attackSpeed.FinalValue;
            _cooldownBar.transform.localScale = new Vector3(1, 1-cooldownPercent, 1);
            yield return null;
        }
        _readyToShoot = true;
        _cooldownBar.transform.localScale = Vector3.one;
        if(_manualControl)
            CustomCursor.Instance.SetCursorState(CursorState.CombatReady, true, false);

    }

    private void Shoot(LaserProjectile projectile, Vector3 target)
    {
        if (GameSystem.Instance.IsPaused) return;
        LaserProjectile laserProjectile = Instantiate(projectile, _cannonShotPoint.position, _cannonShotPoint.rotation);
        laserProjectile.Damage = (int) _damage.FinalValue;
        laserProjectile.TargetPos = target;
        if (projectile == _mouseShootProjectile)
        {
            OnNormalShoot?.Invoke(_cannonShotPoint);
		}
        else if (projectile == _nestShootProjectile)
        {
            OnNestShoot?.Invoke(_cannonShotPoint);
            CameraController.EnqueueCinematic(new CinematicCommand(laserProjectile.transform, 
                Vector3.Distance(_cannonShotPoint.position,target)*0.03f+ 0.5f, null, true));
        }
        else
        {
            Debug.LogWarning($"Unexpected projectile");
        }
    }
    private void OnDestroy()
    {
        GameSystem.OnEnemyAllKilled -= ChargeShotAtNest;
        GameSystem.OnEnemyStartSpawning -= SwitchToManualCannon;
        GameSystem.OnEnemyAllKilled -= SwitchOffManualCannon;
    }

}
