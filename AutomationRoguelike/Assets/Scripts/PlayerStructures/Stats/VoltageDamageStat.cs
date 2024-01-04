using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoltageDamageStat : Stat
{
    public override string GetName()
    {
        return "Voltage Damage";
    }
    public override float BuffValue => 50f;
    public override bool IsBuffable => true;
    protected override void Buff(float multiplier)
    {
        FactorBonusValue += BuffValue * multiplier * 0.01f;
    }
}
