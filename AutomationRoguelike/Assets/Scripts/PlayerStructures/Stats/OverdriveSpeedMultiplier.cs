public class OverdriveSpeedMultiplier : Stat
{
	public override string GetName()
	{
		return "Attack Speed Multiplier";
	}
    public override float BuffValue => 20;
    public override bool IsBuffable => true;
    protected override void Buff(float multiplier)
    {
        FactorBonusValue *= 1 - BuffValue * multiplier * 0.01f;
    }
}
