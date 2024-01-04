using UnityEngine;

public class CryoVariant : MonoVariant
{
    public override bool IsTurret => true;
    [SerializeField] private CryoSlowPercent _cryoSlowPercent;
    [SerializeField] private float _cryoSlowDuration;
    public override void Setup(PlayerStructure playerStructure)
    {
        if (playerStructure.TryGetComponent(out Turret turret))
        {
            SlowOnHit slow = turret.gameObject.AddComponent<SlowOnHit>();
            CryoSlowPercent cryoSlowPercent = turret.gameObject.AddComponent<CryoSlowPercent>();
            cryoSlowPercent.CopyValuesOf(_cryoSlowPercent);
			turret.PlayerStructure.Stats.Add(cryoSlowPercent);
            slow.SlowPercentage = cryoSlowPercent;
            slow.SlowDuration = _cryoSlowDuration;
            slow.SlowIdentifier = "Cryo";
            return;
        }
        Debug.LogWarning($"Cryo applied on something without turret script");
    }
}
