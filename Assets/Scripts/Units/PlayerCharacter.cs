using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class PlayerCharacter : Character
{
    private List<Weapon> availableWeapons = new();
    public Equipment equipment = new();

    [NonSerialized]
    public AttackStats currentAttack1Stats = new(), currentAttack2Stats = new();

    [SerializeField]
    private List<Trait> activeTraits = new();

    public void Initialize()
    {
        equipment = new();
        equipment.Initialize();
    }

    public void Initialize(BasicCharacterTemplates characterTemplate)
    {
        equipment.Initialize();
        characterName = characterTemplate.characterClass.ToString();
        InitializeStats(characterTemplate.parameters);
        InitializeAvailableWeapons(characterTemplate.weaponsCount, characterTemplate.weaponParameters, characterTemplate.weaponSkills);
        RecalculateStats();
    }

    private void InitializeStats(CharacterParameters characterParameters)
    {
        baseCharacterStats.maxHealth = characterParameters.maxHP;
        baseCharacterStats.defence = characterParameters.DEF;
        baseCharacterStats.evasion = characterParameters.EVA;
        baseCharacterStats.SPamount = characterParameters.SP;
        baseCharacterStats.SPregen = characterParameters.SPreg;
        baseCharacterStats.SPmoveCost = characterParameters.SPmovecost;
        baseCharacterStats.speed = characterParameters.SPD;
        baseCharacterStats.tount = characterParameters.PROV;

        currentCharacterStats.maxHealth = baseCharacterStats.maxHealth;
        currentCharacterStats.defence = baseCharacterStats.defence;
        currentCharacterStats.evasion = baseCharacterStats.evasion;
        currentCharacterStats.SPamount = baseCharacterStats.SPamount;
        currentCharacterStats.SPregen = baseCharacterStats.SPregen;
        currentCharacterStats.SPmoveCost = baseCharacterStats.SPmoveCost;
        currentCharacterStats.speed = baseCharacterStats.speed;
        currentCharacterStats.tount = baseCharacterStats.tount;
    }

    private void InitializeAvailableWeapons(int weaponsCount, WeaponParameters[] weaponsParameters, WeaponSkillSet[] weaponsSkillSets)
    {
        for (int i = 0; i < weaponsCount; i++)
        {
            Weapon newWeapon = new();
            newWeapon.Initialize(weaponsParameters[i], weaponsSkillSets[i]);
            availableWeapons.Add(newWeapon);
        }
    }

    public void SetupWeapons()
    {
        if (availableWeapons.Count >= 1)
            equipment.weapon1 = availableWeapons[0];

        if (availableWeapons.Count >= 2)
            equipment.weapon2 = availableWeapons[1];
    }

    public List<Weapon> GetAvailableWeapons()
    {
        List<Weapon> result = new();
        for (int i = 0; i < availableWeapons.Count; i++)
        {
            if (availableWeapons[i] != equipment.weapon1 && availableWeapons[i] != equipment.weapon2)
                result.Add(availableWeapons[i]);
        }
        return result;
    }

    public void AddTrait(TraitData traitData)
    {
        activeTraits.Add(new Trait(traitData));
        RecalculateStats();
    }

    public void RemoveTrait(Trait trait)
    {
        activeTraits.Remove(trait);
        RecalculateStats();
    }

    public void ProcessTurnEnd()
    {
        foreach (var trait in activeTraits)
        {
            trait.OnTurnEnd();
            if (trait.IsExpired) RemoveTrait(trait);
        }
    }

    public void RecalculateStats()
    {
        float healthRatio = currentHealth / (float)currentCharacterStats.maxHealth;

        currentCharacterStats.maxHealth = baseCharacterStats.maxHealth;
        currentCharacterStats.defence = baseCharacterStats.defence;
        currentCharacterStats.evasion = baseCharacterStats.evasion;
        currentCharacterStats.SPamount = baseCharacterStats.SPamount;
        currentCharacterStats.SPregen = baseCharacterStats.SPregen;
        currentCharacterStats.SPmoveCost = baseCharacterStats.SPmoveCost;
        currentCharacterStats.speed = baseCharacterStats.speed;
        currentCharacterStats.tount = baseCharacterStats.tount;

        foreach (var trait in activeTraits)
        {
            foreach (var effect in trait.Data.effects)
            {
                ApplyTraitEffect(effect);
            }
        }

        currentHealth = Mathf.RoundToInt(currentCharacterStats.maxHealth * healthRatio);
        currentHealth = Mathf.Clamp(currentHealth, 1, currentCharacterStats.maxHealth);
    }

    private void ApplyTraitEffect(TraitEffect effect)
    {
        switch (effect.statType)
        {
            case StatType.Health:
                currentCharacterStats.maxHealth += GetTraitModifierStat(baseCharacterStats.maxHealth, effect);
                break;
            case StatType.Defence:
                currentCharacterStats.defence += GetTraitModifierStat(baseCharacterStats.defence, effect);
                break;
            case StatType.Evasion:
                currentCharacterStats.evasion += GetTraitModifierStat(baseCharacterStats.evasion, effect);
                break;
            case StatType.SPamount:
                currentCharacterStats.SPamount += GetTraitModifierStat(baseCharacterStats.SPamount, effect);
                break;
            case StatType.SPregen:
                currentCharacterStats.SPregen += GetTraitModifierStat(baseCharacterStats.SPregen, effect);
                break;
            case StatType.SPmoveCost:
                currentCharacterStats.SPmoveCost += GetTraitModifierStat(baseCharacterStats.SPmoveCost, effect);
                break;
            case StatType.Speed:
                currentCharacterStats.speed += GetTraitModifierStat(baseCharacterStats.speed, effect);
                break;
            case StatType.Tount:
                currentCharacterStats.tount += GetTraitModifierStat(baseCharacterStats.tount, effect);
                break;
        }
    }


    private int GetTraitModifierStat(int baseStat, TraitEffect effect)
    {
        switch (effect.effectType)
        {
            case TraitEffectType.Additive:
                return Mathf.RoundToInt(effect.value);

            case TraitEffectType.Multiplicative:
                return Mathf.RoundToInt(baseStat * (float)effect.value / 100);

            default:
                return 0;
        }

    }

    public List<Trait> GetTraits(TraitType traitType)
    {
        List<Trait> result = new();
        foreach (Trait trait in activeTraits)
        {
            if (trait.Data.traitType == traitType)
                result.Add(trait);
        }
        return result;
    }

    internal bool HasTrait(TraitData randomTraitData)
    {
        foreach(Trait trait in activeTraits)
        {
            if(trait.Data == randomTraitData)
                return true;
        }
        return false;
    }
}
