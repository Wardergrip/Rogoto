using System.Collections;
using UnityEditor;
using UnityEngine;

public class Rootnest : EnemyNest
{
	[SerializeField] private HealthBar _inactiveHealthbarPrefab;
	[SerializeField] private Animator _animator;

	[SerializeField] private GameObject _shield;
	[SerializeField] private Material _shieldMaterial;
	[SerializeField] private float _secondsUntilShield = 7;

	private bool _active = true;

	protected override void Awake()
	{
		base.Awake();
		GameSystem.OnRewardClaimed += IncreaseLevel;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		s_PreviousNestPos = Base.Position;
	}

	private void IncreaseLevel()
	{
		++_level;
		_healthBar.SetLevel(_level);
	}

	public override void DamageNest()
	{
		_health.Damage(1);
		if (_health.HealthAmount <= 0)
		{
			_secondsUntilDamageEvent += _secondsUntilShield;
			Death();
		}


		if (_health.HealthAmount > 0 && _active)
			_animator.Play("EnemyRootNest_GoingDown");

		//GameSystem.Instance.NestDamaged();
		StartCoroutine(WaitUntilDamageEvent(_secondsUntilDamageEvent));
	}

	public void Die()
	{
		Death();
	}

	protected override void Death()
	{
		CameraController.EnqueueCinematic(new CinematicCommand(transform, 8.2f, 3f));
		_active = false;
		_animator.Play("EnemyRootNest_CompletelyDown");

		GameSystem.OnRewardClaimed -= DamageNest;
		_healthBarColorOverride = Color.white;
		ActiveNests.Remove(this);

		StartCoroutine(WaitUntilShield(_secondsUntilShield));
	}

	private IEnumerator WaitUntilShield(float seconds)
	{
		yield return new WaitForSeconds(seconds);
		Shield();
	}

	public void Shield()
	{
		_health.DecreaseMaxHealth(1);
		_health.Heal(1);

		_healthBar = Instantiate(_inactiveHealthbarPrefab, transform.position, Quaternion.identity);
		_healthBar.SetUpHealthBar(transform, _health, _healthBarColorOverride, _level);

		_shield.SetActive(true);
		GetComponentInChildren<SkinnedMeshRenderer>().material = _shieldMaterial;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(Rootnest))]
public class Rootnest_Editor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		Rootnest script = (Rootnest)target;

		if (GUILayout.Button("Damage"))
			script.DamageNest();
		if (GUILayout.Button("Die"))
			script.Die();
	}
}
#endif