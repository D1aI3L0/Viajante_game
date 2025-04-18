using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PlayerCharacter : Character
{
    public int level = 1;
    public Equipment equipment;
    public AttackStats currentAttack1Stats, currentAttack2Stats, maxAttack1Stats, maxAttack2Stats;

    public void UpdateStats()
    {
        UpdateSurvivalStats();
        UpdateAttack1Stats();
        UpdateAttack2Stats();
        UpdateOtherStats();
    }

    public void UpdateSurvivalStats()
    {
        maxSurvivalStats.health = baseSurvivalStats.health;
        currentSurvivalStats.health = maxSurvivalStats.health;
        maxSurvivalStats.defence = baseSurvivalStats.defence;
        currentSurvivalStats.defence = maxSurvivalStats.defence;
        maxSurvivalStats.evasion = baseSurvivalStats.evasion;
        currentSurvivalStats.evasion = maxSurvivalStats.evasion;
    }

    public void UpdateAttack1Stats()
    {
        maxAttack1Stats.attack = (int)Math.Round(equipment.weapon1.attackStats.attack * (1 + equipment.weapon1.attackBonus / 100));
        currentAttack1Stats.attack = maxAttack1Stats.attack;
        maxAttack1Stats.accuracy = (int)Math.Round(equipment.weapon1.attackStats.accuracy * (1 + equipment.weapon1.accuracyBonus / 100));
        currentAttack1Stats.accuracy = maxAttack1Stats.accuracy;
        maxAttack1Stats.critRate = (int)Math.Round(equipment.weapon1.attackStats.critRate * (1 + equipment.weapon1.critBonus / 100));
        currentAttack1Stats.critRate = maxAttack1Stats.critRate;
    }
    
    public void UpdateAttack2Stats()
    {
        maxAttack2Stats.attack = (int)Math.Round(equipment.weapon2.attackStats.attack * (1 + equipment.weapon2.attackBonus / 100));
        currentAttack2Stats.attack = maxAttack2Stats.attack;
        maxAttack2Stats.accuracy = (int)Math.Round(equipment.weapon2.attackStats.accuracy * (1 + equipment.weapon2.accuracyBonus / 100));
        currentAttack2Stats.accuracy = maxAttack2Stats.accuracy;
        maxAttack2Stats.critRate = (int)Math.Round(equipment.weapon2.attackStats.critRate * (1 + equipment.weapon2.critBonus / 100));
        currentAttack2Stats.critRate = maxAttack2Stats.critRate;
    }

    public void UpdateOtherStats()
    {
        maxOtherStats.initiative = baseOtherStats.initiative;
        currentOtherStats.initiative = maxOtherStats.initiative;
        maxOtherStats.agro = baseOtherStats.agro;
        currentOtherStats.agro = maxOtherStats.agro;
        maxOtherStats.endurance.amount = baseOtherStats.endurance.amount;
        currentOtherStats.endurance.amount = maxOtherStats.endurance.amount;
        maxOtherStats.endurance.regen = baseOtherStats.endurance.regen;
        currentOtherStats.endurance.regen = maxOtherStats.endurance.regen;
        maxOtherStats.endurance.moveCost = baseOtherStats.endurance.moveCost;
        currentOtherStats.endurance.moveCost = maxOtherStats.endurance.moveCost;
    }
}
