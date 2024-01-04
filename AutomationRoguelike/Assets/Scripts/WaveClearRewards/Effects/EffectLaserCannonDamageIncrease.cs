using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectLaserCannonDamageIncrease : Effect
{
    [SerializeField] private int _damageIncrease;
    public override void Excecute()
    {
        FindObjectOfType<LaserCannon>().GetComponent<Damage>().FlatBonusValue+=_damageIncrease;
    }
}
