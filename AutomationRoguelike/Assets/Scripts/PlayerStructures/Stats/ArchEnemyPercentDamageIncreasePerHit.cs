public class ArchEnemyPercentDamageIncreasePerHit : Stat
{
	public override string GetName()
	{
		return "Archenemy Damage";
	}
    public override float BuffValue => 30f;
    public override bool IsBuffable => true;
    protected override void Buff(float multiplier)
    {
        FactorBonusValue += BuffValue * multiplier * 0.01f;
    }
}
