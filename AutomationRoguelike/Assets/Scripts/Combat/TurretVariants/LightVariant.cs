using UnityEngine;

public class LightVariant : MonoVariant
{
    [SerializeField] private float _damageBonus = 0.5f;
    [SerializeField] private float _timeBetweenAttacksBonus = 0.5f;

    public override void Setup(PlayerStructure playerStructure)
    {
        if (playerStructure.TryGetComponent(out Turret turret))
        {
            turret.Damage.FactorBonusValue *= _damageBonus;
            turret.TimeBetweenAttacks.FactorBonusValue *= _timeBetweenAttacksBonus;
            return;
        }
        Debug.LogWarning($"LightVariant applied on something without turret script");
    }
    public override bool IsTurret { get => true; }
}
