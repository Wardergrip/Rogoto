using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenVariant : MonoVariant
{
    [SerializeField] private GoldenStat _goldPerKill;
    public override bool IsTurret => true;

    public override void Setup(PlayerStructure playerStructure)
    {
        if (playerStructure.TryGetComponent(out Turret turret))
        {
            GoldOnKill gok = turret.gameObject.AddComponent < GoldOnKill>();
            gok.GoldPerKillStat = _goldPerKill;
            turret.PlayerStructure.Stats.Add(_goldPerKill);
            return;
        }
        Debug.LogWarning($"Golden applied on something without turret script");
    }
}
