public class Damage : Stat
{
    public override string GetName()
    {
        return "Damage";
    }
    public override float BuffValue => 20f;
	public override bool IsBuffable => true;
	protected override void Buff(float multiplier)
    {
		FactorBonusValue += BuffValue*multiplier * 0.01f;
    }
}
