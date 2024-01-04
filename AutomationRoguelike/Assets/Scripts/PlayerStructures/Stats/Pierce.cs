[System.Obsolete]
public class Pierce : Stat
{
    public override string GetName()
    {
        return "Armor Pierce";
    }
    public override float BuffValue => 100;
	public override bool IsBuffable => true;
	protected override void Buff(float multiplier)
    {
		FactorBonusValue += BuffValue * 0.01f;
    }
}