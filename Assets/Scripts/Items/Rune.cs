using System.Collections.Generic;

public class Rune : Item
{
    public int attackBonus;
    public int critBonus;
    public int accuracyBonus;
    public StatusEffect statusEffect;
    public int statusChance;

    public List<HexDirection> runeConnections = new();
}