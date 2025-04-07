using System;
using System.Collections.Generic;

[Serializable]
public struct CharactetStats
{
    public int health;
    public int speed;
    public int armor;
    public int attack1, attack2;
    public int critRate;
    public int critResist, physResist, magRerist;
    public int evasion;
    public int accuracy;
}


public class Character : Unit
{
    public string characterName;
    public static Character characterPrefab;
    public enum CharacterType
    {
        Player,
        Enemy
    }

    public CharacterType characterType;
    public CharactetStats currentCharactetStats, maxCharacterStats;

    override public bool IsValidDestination(HexCell cell, bool useUnitCollision = false)
    {
        return base.IsValidDestination(cell);
    }
}