using System;
using UnityEngine;

public class MonoExplosion : MonoBehaviour
{
	public Projectile Projectile { get; set; }
	[Header("Fallback values")]
	[SerializeField] private int _damage = 0;
	[SerializeField] private float _range = 1;
	protected event Action OnRangeChange;

	public float Range { get => _range; set { _range = value; OnRangeChange?.Invoke(); } }
	public int Damage { get => _damage; set => _damage = value; }
}
