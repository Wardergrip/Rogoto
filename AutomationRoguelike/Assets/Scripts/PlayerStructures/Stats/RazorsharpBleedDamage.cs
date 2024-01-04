public class RazorsharpBleedDamage : Stat
{
	public override string GetName()
	{
		return "Bleed Damage";
	}
    public override float BuffValue => 50f;
    public override bool IsBuffable => true;
    protected override void Buff(float multiplier)
    {
        FactorBonusValue += BuffValue * multiplier * 0.01f;
    }
}
