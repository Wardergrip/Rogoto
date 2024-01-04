using UnityEngine;

public class DamagePopUpSpawner : MonoBehaviour
{
	[SerializeField] private DamagePopUp _damagePopUp;
	private Color _spawnColor = Color.white;
	[SerializeField] private Vector3 _offset;

	[Header("Damage Colors")]
	[SerializeField] private Color _physicalColor;
	[SerializeField] private Color _bleedColor;
	[SerializeField] private Color _toxicColor;
	[SerializeField] private Color _voltageColor;
	[SerializeField] private Color _explosiveColor;

	public void SpawnDamagePopUp(int damage, Health.DamageType dt)
	{
		Health.DamageType damageType = dt;
		switch (damageType)
		{
			case Health.DamageType.Physical:
				_spawnColor = _physicalColor;
				break;
			case Health.DamageType.Bleed:
				_spawnColor = _bleedColor;
				break;
			case Health.DamageType.Toxic:
				_spawnColor = _toxicColor;
				break;
			case Health.DamageType.Voltage:
				_spawnColor = _voltageColor;
				break;
			case Health.DamageType.Explosive:
				_spawnColor = _explosiveColor;
				break;
			default:
				_spawnColor = Color.white;
				break;
		}

		DamagePopUp damagePopUp = Instantiate(_damagePopUp,transform.position + _offset,Quaternion.identity);
		damagePopUp.SetNumberText(damage);
		damagePopUp.SetColor(_spawnColor);
	}
}
