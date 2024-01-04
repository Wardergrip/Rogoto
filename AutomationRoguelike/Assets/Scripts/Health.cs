using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
	//Variables
	[SerializeField] private int _maxHealth = 100;
	private int _healthAmount;
	private readonly float _levelScaleCoefficient = 1.25f;

	//Events
	public UnityEvent<int> OnTookDamage;
	public UnityEvent<int, Health.DamageType> OnTookDamageType;
	public UnityEvent<int> OnGainedHealth;
	public UnityEvent OnDied;
	public UnityEvent<int> OnArmorReduced;

	public int HealthAmount { get => _healthAmount; }
	public int MaxHealth { get => _maxHealth; private set => _maxHealth = value; }

	public enum DamageType : int
	{
		Physical = 0,
		Bleed = 1,
		Toxic = 2,
		Voltage = 3,
		Explosive = 4
	}

	/// <summary>
	/// Changes max and current
	/// </summary>
	/// <param name="amount"></param>
	public void AdjustHealth(int amount)
	{
		_maxHealth = amount;
		_healthAmount = amount;
	}

	public void ScaleHealthByLevel(int level)
	{
		float scaledHealth = MaxHealth * Mathf.Max(1, Mathf.Pow(_levelScaleCoefficient, level - 1));
		_maxHealth = (int)scaledHealth;
		_healthAmount = MaxHealth;
	}

	void Awake()
	{
		_healthAmount = MaxHealth;
	}

	public int Damage(int amount, DamageType damageType = 0)
	{
		int max = HealthAmount;
		_healthAmount = (HealthAmount - amount);

		_healthAmount = Mathf.Clamp(_healthAmount, 0, max);

		OnTookDamage?.Invoke(max - _healthAmount);
		OnTookDamageType?.Invoke(amount, damageType);
		if (HealthAmount <= 0)
			OnDied?.Invoke();
		return HealthAmount;
	}

	public int Heal(int amount)
	{
		_healthAmount += amount;
		int previous = _healthAmount;
		_healthAmount = Mathf.Clamp(_healthAmount, 0, MaxHealth);

		OnGainedHealth?.Invoke(_healthAmount - previous);
		return HealthAmount;
	}
	public int HealPercentage(float percentage)
	{
		return Heal((int)(percentage * _maxHealth));
	}

	public void IncreaseMaxHealth(int amount)
	{
		_maxHealth += amount;
		OnTookDamage?.Invoke(0);
	}
	public void DecreaseMaxHealth(int amount)
	{
		_maxHealth -= amount;
		_healthAmount = Mathf.Clamp(_healthAmount, 0, _maxHealth);
		OnTookDamage?.Invoke(0);
	}

	public void InstaKill()
	{
		_healthAmount = 0;
		OnDied?.Invoke();
	}
}
