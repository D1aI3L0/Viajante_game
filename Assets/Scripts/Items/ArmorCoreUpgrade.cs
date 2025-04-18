using System;
using UnityEngine;

[Serializable]
public enum SurvivalStatType
{
    Health,
    Defence,
    Evasion
}

[Serializable]
public class ArmorCoreUpgrade
{
    public SurvivalStatType statType;
    public int bonusValue;
    public bool isBurned;
    public Vector2Int gridPosition;
}