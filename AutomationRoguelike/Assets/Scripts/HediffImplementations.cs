using UnityEngine;

// Meant to be used as baseclasses
#region Basic_Hediffs
public class DamageOverTime : Hediff
{
    protected Health _health;
    protected int _damagePerTick;
    protected Health.DamageType _damageType = 0;
    protected int _pierce;
    public DamageOverTime(string identifier, Health health, int damagePerTick, int pierce, float timeLeft, bool isStackable) : base(identifier, timeLeft,isStackable )
    {
        _health = health;
        Debug.Assert(health != null);
        _damagePerTick = damagePerTick;
        Debug.Assert(_damagePerTick > 0);
        _pierce = pierce;
        Debug.Assert(_pierce >= 0);
    }

    public override void Tick()
    {
        _health.Damage(_damagePerTick, _damageType);
    }
}
#endregion
#region Implementations
// used for razorsharp
public class Bleed : DamageOverTime
{
    public Bleed(Health health, int damagePerTick, int pierce, float timeLeft) : base("Bleed", health, damagePerTick, pierce, timeLeft, true)
    {
        _damageType = Health.DamageType.Bleed;
	}

    public override void Reapply(Hediff hediff)
    {
        Bleed bleed = (Bleed)hediff;
        _damagePerTick += bleed._damagePerTick;
        TimeLeft = Mathf.Max(_damagePerTick, bleed._damagePerTick);
    }
}
#endregion