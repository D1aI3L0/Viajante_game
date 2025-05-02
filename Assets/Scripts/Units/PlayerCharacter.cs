using System;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[Serializable]
public class PlayerCharacter : Character
{
    private List<Weapon> availableWeapons = new();
    public Equipment equipment = new();
    public SurvivalStats baseSurvivalStats;
    public OtherStats baseOtherStats;
    [NonSerialized]
    public AttackStats currentAttack1Stats, currentAttack2Stats, maxAttack1Stats, maxAttack2Stats;

    public void Initialize()
    {
        equipment = new();
        equipment.Initialize();
    }

    public void InitializeStats()
    {
        InitializeSurvivalStats();
        InitializeAttack1Stats();
        InitializeAttack2Stats();
        InitializeOtherStats();
    }

    public void InitializeSurvivalStats()
    {
        maxSurvivalStats.health = baseSurvivalStats.health;
        currentSurvivalStats.health = maxSurvivalStats.health;
        maxSurvivalStats.defence = baseSurvivalStats.defence;
        currentSurvivalStats.defence = maxSurvivalStats.defence;
        maxSurvivalStats.evasion = baseSurvivalStats.evasion;
        currentSurvivalStats.evasion = maxSurvivalStats.evasion;
    }

    public void InitializeAttack1Stats()
    {
        maxAttack1Stats.attack = equipment.weapon1.attackStats.attack;
        currentAttack1Stats.attack = maxAttack1Stats.attack;
        maxAttack1Stats.accuracy = equipment.weapon1.attackStats.accuracy;
        currentAttack1Stats.accuracy = maxAttack1Stats.accuracy;
        maxAttack1Stats.critRate = equipment.weapon1.attackStats.critRate;
        currentAttack1Stats.critRate = maxAttack1Stats.critRate;
    }
    
    public void InitializeAttack2Stats()
    {
        maxAttack2Stats.attack = equipment.weapon2.attackStats.attack;
        currentAttack2Stats.attack = maxAttack2Stats.attack;
        maxAttack2Stats.accuracy = equipment.weapon2.attackStats.accuracy;
        currentAttack2Stats.accuracy = maxAttack2Stats.accuracy;
        maxAttack2Stats.critRate = equipment.weapon2.attackStats.critRate;
        currentAttack2Stats.critRate = maxAttack2Stats.critRate;
    }

    public void InitializeOtherStats()
    {
        maxOtherStats.initiative = baseOtherStats.initiative;
        currentOtherStats.initiative = maxOtherStats.initiative;
        maxOtherStats.tount = baseOtherStats.tount;
        currentOtherStats.tount = maxOtherStats.tount;
        maxOtherStats.endurance.amount = baseOtherStats.endurance.amount;
        currentOtherStats.endurance.amount = maxOtherStats.endurance.amount;
        maxOtherStats.endurance.regen = baseOtherStats.endurance.regen;
        currentOtherStats.endurance.regen = maxOtherStats.endurance.regen;
        maxOtherStats.endurance.moveCost = baseOtherStats.endurance.moveCost;
        currentOtherStats.endurance.moveCost = maxOtherStats.endurance.moveCost;
    }

    public List<Weapon> GetAvailableWeapons()
    {
        List<Weapon> result = new();
        for(int i = 0; i < availableWeapons.Count; i++)
        {
            if(availableWeapons[i] != equipment.weapon1 && availableWeapons[i] != equipment.weapon2)
                result.Add(availableWeapons[i]);
        }
        return result;
    }
}
