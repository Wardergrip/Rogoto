using UnityEngine;

public class EffectBaseRepair : Effect
{
    [SerializeField] private float _repairAmount = 0.1f;
    public override void Excecute()
    {
        Base instance = Base.Instance;
        if (instance == null)
        {
            return;
        }
        instance.Health.HealPercentage(_repairAmount);
    }

	public override bool IsAvailable()
	{
        Base instance = Base.Instance;
        return instance != null && instance.Health.HealthAmount < instance.Health.MaxHealth;
	}
}
