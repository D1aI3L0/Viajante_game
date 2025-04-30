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
    public int tount;
    public Endurance endurance;
}


[Serializable]
public class Character
{
    public string characterName;
    public int level = 1;
    public SurvivalStats currentSurvivalStats, maxSurvivalStats;
    public OtherStats currentOtherStats, maxOtherStats;
}