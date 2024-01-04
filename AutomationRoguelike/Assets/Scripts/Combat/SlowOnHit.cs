using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SlowHediff : Hediff
{
    private HediffHandler _handler;
    private float _percentage;

    public SlowHediff(string identifier, HediffHandler handler, float timeLeft, float percentage) : base(identifier, timeLeft, false)
    {
        _percentage = percentage;
        _handler = handler;
    }

    public override void Apply()
    {
        _handler.Enemy.SlowMovement(_percentage);
    }

    public override void Reapply(Hediff hediff)
    {
        Remove();
        SlowHediff newSLow = hediff as SlowHediff;
        if (newSLow.TimeLeft > TimeLeft)
            TimeLeft= newSLow.TimeLeft;

        if (newSLow._percentage > _percentage)
            _percentage = newSLow._percentage;
        _percentage = 1 / _percentage;
        Apply();

    }
    public override void Remove()
    {
        _percentage = 1/_percentage;
        Apply();
    }


}
public class SlowOnHit : MonoBehaviour
{
	private List<Turret> _turrets = new();
	[SerializeField] private Stat _slowPercentage;
    [SerializeField] private float _slowDuration;
    [SerializeField] private string _slowIdentifier;
    public float SlowDuration { get => _slowDuration; set => _slowDuration = value; }
    public Stat SlowPercentage { get => _slowPercentage; set => _slowPercentage = value; }
    public string SlowIdentifier { get => _slowIdentifier; set => _slowIdentifier = value; }

    private void Awake()
    {
		_turrets = GetComponents<Turret>().ToList();
		foreach (var turret in _turrets)
			turret.OnProjectileHit += SlowHit;
	}

    private void SlowHit(ProjectileHitData obj)
    {
        obj.HediffHandler.AddHediff(new SlowHediff(_slowIdentifier, obj.HediffHandler, _slowDuration, _slowPercentage.FinalValue));
    }
}
