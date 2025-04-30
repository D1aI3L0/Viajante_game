using System;
using UnityEngine;

[Serializable]
public enum SurvivalStatType
{
    Balanced,
    Health,
    Defence,
    Evasion
}

[Serializable]
public class ArmorCoreUpgrade : Upgrade
{
    public bool isBurned;
    public SurvivalStatType statType;
    public int bonusValue;
}