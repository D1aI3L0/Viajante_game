using System;

[Serializable]
public class CharacterStats
{
    public int maxHealth;
    public int defence;
    public int evasion;
    public int SPamount, SPregen, SPmoveCost;
    public int speed;
    public int tount;
}


[Serializable]
public class Character
{
    public string characterName;
    public int level = 1;
    public CharacterStats currentCharacterStats = new(), baseCharacterStats = new();
    public int currentHealth;
}