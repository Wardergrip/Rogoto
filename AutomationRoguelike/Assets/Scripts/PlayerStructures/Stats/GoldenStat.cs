using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldenStat : Stat
{
    public override string GetName()
    {
        return "Gold On Kill";
    }
    public override float BuffValue => 100f;
    public override bool IsBuffable => true;
    protected override void Buff(float multiplier)
    {
        FactorBonusValue += BuffValue * multiplier * 0.01f;
    }
}
