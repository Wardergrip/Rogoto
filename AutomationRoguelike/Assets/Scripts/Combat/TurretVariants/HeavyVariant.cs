using UnityEngine;

public class HeavyVariant : MonoVariant
{
    [SerializeField] private float _damageBonus = 2f;
    [SerializeField] private float _timeBetweenAttacksBonus = 1.5f;

    public override void Setup(PlayerStructure playerStructure)
    {
        if (playerStructure.TryGetComponent(out Turret turret))
        {
            turret.Damage.FactorBonusValue *= _damageBonus;
            turret.TimeBetweenAttacks.FactorBonusValue *= _timeBetweenAttacksBonus;
            return;
        }
        Debug.LogWarning($"HeavyVariant applied on something without turret script");
    }

    public override bool IsTurret { get => true; }
}
