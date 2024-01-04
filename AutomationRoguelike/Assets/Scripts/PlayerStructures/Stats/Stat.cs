using System;
using UnityEngine;

[System.Serializable]
public struct TieredValues
{
    public float this[int idx]
    {
        get
        {
            return idx switch
            {
                1 => Tier1Value,
                2 => Tier2Value,
                3 => Tier3Value,
                _ => throw new IndexOutOfRangeException("Index out of range."),
            };
        }
        set
        {
            switch (idx)
            {
                case 1:
                    Tier1Value = value;
                    break;
                case 2:
                    Tier2Value = value;
                    break;
                case 3:
                    Tier3Value = value;
                    break;
                default:
                    throw new IndexOutOfRangeException("Index out of range.");
            }
        }
    }

    public float Tier1Value;
    public float Tier2Value;
    public float Tier3Value;
}

public abstract class Stat : MonoBehaviour
{
    [SerializeField] private TieredValues _baseValue;
    public int CurrentTier { get; private set; } = 1;
    public float BaseValue { get => _baseValue[CurrentTier]; set => _baseValue[CurrentTier] = value; }
    public float FlatBonusValue { get; set; } = 0;
    public float FactorBonusValue { get; set; } = 1.0f;
    public float FinalValue { get => (_baseValue[CurrentTier] + FlatBonusValue) * FactorBonusValue;  }
    public float PredictedNextValue { get => (_baseValue[Mathf.Clamp(CurrentTier + 1,1,3)] + FlatBonusValue) * FactorBonusValue; }
    public virtual float BuffValue { get => 0.0f; }
    public abstract string GetName();
    public void SetTier(int tier) 
    {
        CurrentTier = tier switch
        {
            1 or 2 or 3 => tier,
            _ => throw new Exception("Tier is not anticipated. Please make sure the implemeentation supports it"),
        };
    }

    /// <summary>
    /// Called by effects and other things to allow them  to buff this stat
    /// </summary>
    public void RootBuff(float multiplier)
    {
        Buff(multiplier);
        OnBuff?.Invoke(this);
        OnVisualNeedsRefresh?.Invoke(this);
	}
	/// <summary>
	/// Called by effects and other things to allow them  to buff this stat
	/// </summary>
	protected virtual void Buff(float multiplier) { }
    public virtual bool IsBuffable { get => false; }

    public event Action<Stat> OnBuff;
    public event Action<Stat> OnVisualNeedsRefresh;
    public void ForceRefreshVisual()
    {
		OnVisualNeedsRefresh?.Invoke(this);
    }

    /// <summary>
    /// Intended to only be called on other that is same type
    /// </summary>
    /// <param name="other"></param>
    public void CopyValuesOf(Stat other)
    {
        _baseValue = other._baseValue;
        FlatBonusValue = other.FlatBonusValue;
        FactorBonusValue = other.FactorBonusValue;
	}
}
