public class CryoSlowPercent : Stat
{
	public override string GetName()
	{
		return "Slow Strength";
	}
    public override float BuffValue => 5;
    public override bool IsBuffable => true;
    protected override void Buff(float multiplier)
    {
        FactorBonusValue *= 1 - BuffValue * multiplier * 0.01f;
    }
}
