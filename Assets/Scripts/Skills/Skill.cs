using System.Collections.Generic;
using UnityEngine;

public class Skill
{
    public Sprite icon;
    public string skillName;
    public int minDamagePercent, maxDamagePercent;
    public int accuracy;
    public int critRate;

    public StatusEffect statusEffect;
    public int effectChance;
    public Dictionary<StatusEffect, int> extraEffects = new();
}