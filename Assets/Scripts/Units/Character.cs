using System;

[Serializable]
public class SurvivalStats
{
    public int health;
    public int defence;
    public int evasion;
}

[Serializable]
public class AttackStats
{
    public int attack;
    public int accuracy;
    public int critRate;
}

[Serializable]
public struct Endurance
{
    public int amount, regen, moveCost;
}

[Serializable]
public class OtherStats
{
    public int initiative;
    public int agro;
    public Endurance endurance;
}


public class Character : Unit
{
    public string characterName;
    public SurvivalStats currentSurvivalStats, maxSurvivalStats, baseSurvivalStats;
    public OtherStats currentOtherStats, maxOtherStats, baseOtherStats;

    override public bool IsValidDestination(HexCell cell)
    {
        return base.IsValidDestination(cell);
    }
}