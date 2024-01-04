using UnityEngine;

public class EffectIncreaseLuck : Effect
{
    [SerializeField] private int _increase = 10;

    public override void Excecute()
    {
        GameSystem.Instance.Luck += _increase;
    }
}
