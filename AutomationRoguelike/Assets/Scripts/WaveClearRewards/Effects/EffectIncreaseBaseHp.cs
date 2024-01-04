using UnityEngine;

public class EffectIncreaseBaseHp : Effect
{
    [SerializeField] private int _amount = 10;
    public override void Excecute()
    {
        Base instance = Base.Instance;
        if (instance == null)
        {
            return;
        }
        instance.Health.IncreaseMaxHealth(_amount);
        instance.Health.Heal(_amount);
        BaseHealthBarIdentifier.Instance.HealthBar.SeperationsInHealthBar += 3;
    }
}
