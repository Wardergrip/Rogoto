using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLaserAttackSpeedIncrease : Effect
{
    [SerializeField] private float _attackSpeedIncrease;

    public override void Excecute()
    {
        FindObjectOfType<LaserCannon>().GetComponent<TimeBetweenAttacks>().FactorBonusValue*= 1- _attackSpeedIncrease;
    }
}
