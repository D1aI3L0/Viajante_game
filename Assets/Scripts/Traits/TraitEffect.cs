[System.Serializable]
public class TraitEffect
{
    public TraitEffectType effectType;
    public StatType statType;
    public int value;
}

public enum StatType
{
    Health,
    Defence,
    Evasion,
    SPamount,
    SPregen,
    SPmoveCost,
    Speed,
    Tount
}

public enum TraitEffectType
{
    Additive,
    Multiplicative
}