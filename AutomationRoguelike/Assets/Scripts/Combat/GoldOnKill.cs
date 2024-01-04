using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldOnKill : MonoBehaviour
{
    private Turret _turret;
    public GoldenStat GoldPerKillStat { get; set; }

    private void Awake()
    {
        _turret = GetComponent<Turret>();
        _turret.OnProjectileKill += GetGold;
       
    }
    private void GetGold(ProjectileHitData projectileHitData)
    {
        EconomyManager.Instance.AddGold((int)GoldPerKillStat.FinalValue);
    }
}
