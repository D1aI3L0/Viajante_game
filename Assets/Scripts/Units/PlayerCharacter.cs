using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class PlayerCharacter : Character
{
    public CharacterClass characterClass;
    private List<Weapon> availableWeapons = new();
    public Equipment equipment = new();
    public int currentHealth;

    [SerializeField]
    private List<Trait> activeTraits = new();

    public void Initialize()
    {
        equipment = new();
        equipment.Initialize();
    }

    public void Initialize(BasicCharacterTemplates characterTemplate)
    {
        characterClass = characterTemplate.characterClass;
        characterName = characterTemplate.characterClass.ToString();
        equipment.Initialize();
        InitializeStats(characterTemplate.parameters);
        InitializeAvailableWeapons(characterTemplate.weaponsCount, characterTemplate.weaponParameters, characterTemplate.weaponSkills);
        RecalculateStats();
    }

    public void InitializeStats(CharacterParameters characterParameters)
    {
        baseCharacterStats.maxHealth = currentCharacterStats.maxHealth = characterParameters.maxHP;
        baseCharacterStats.defence = characterParameters.DEF;
        baseCharacterStats.evasion = characterParameters.EVA;
        baseCharacterStats.SPamount = characterParameters.SP;
        baseCharacterStats.SPregen = characterParameters.SPreg;
        baseCharacterStats.SPmoveCost = characterParameters.SPmovecost;
        baseCharacterStats.speed = characterParameters.SPD;
        baseCharacterStats.tount = characterParameters.PROV;
        currentHealth = baseCharacterStats.maxHealth;
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

    public int GetWeaponID(Weapon weapon)
    {
        for (int i = 0; i < availableWeapons.Count; i++)
        {
            if (availableWeapons[i] == weapon)
                return i;
        }
        return -1;
    }

    public Weapon GetWeaponByID(int id)
    {
        if (id < 0 || id >= availableWeapons.Count)
            return null;

        return availableWeapons[id];
    }

    public void AddTrait(TraitData traitData)
    {
        if (traitData != null)
        {
            activeTraits.Add(new Trait(traitData));
            RecalculateStats();
        }
    }

    public void RemoveTrait(Trait trait)
    {
        if (trait != null)
        {
            activeTraits.Remove(trait);
            RecalculateStats();
        }
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

        ApplyArmorCoreBonuses();

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

    private void ApplyArmorCoreBonuses()
    {
        currentCharacterStats.maxHealth += GetArmorCoreModifierStat(baseCharacterStats.maxHealth, equipment.armorCore.HealthBonus);
        currentCharacterStats.evasion += GetArmorCoreModifierStat(baseCharacterStats.evasion, equipment.armorCore.EvasionBonus);
        currentCharacterStats.defence += GetArmorCoreModifierStat(baseCharacterStats.defence, equipment.armorCore.DefenceBonus);
    }

    private int GetArmorCoreModifierStat(int baseStat, int bonus)
    {
        return Mathf.RoundToInt(baseStat * (float)bonus / 100);
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
        foreach (Trait trait in activeTraits)
        {
            if (trait.Data == randomTraitData)
                return true;
        }
        return false;
    }

    public Weapon GetWeaponInEquipnentByID(int id)
    {
        if(id == 0)
            return equipment.weapon1;
        if(id == 1)
            return equipment.weapon2;
        
        return null;
    }
    //=================================================================================================
    //                                      Сохранение и загрузка
    //=================================================================================================
    public override void Save(BinaryWriter writer)
    {
        base.Save(writer);
        writer.Write((int)characterClass);

        writer.Write(availableWeapons.Count);

        foreach (Weapon weapon in availableWeapons)
        {
            weapon.Save(writer);
        }

        equipment.Save(writer, this);

        var positiveTraits = GetTraits(TraitType.Positive);
        writer.Write(positiveTraits.Count);
        foreach (Trait trait in positiveTraits)
        {
            writer.Write(RecruitingController.Instance.GetTraitID(trait.Data));
        }

        var negativeTraits = GetTraits(TraitType.Negatine);
        writer.Write(negativeTraits.Count);
        foreach (Trait trait in negativeTraits)
        {
            writer.Write(RecruitingController.Instance.GetTraitID(trait.Data));
        }

        writer.Write(currentHealth);
    }

    public override void Load(BinaryReader reader)
    {
        base.Load(reader);

        characterClass = (CharacterClass)reader.ReadInt32();
        characterName = characterClass.ToString();

        BasicCharacterTemplates characterTemplate = RecruitingController.GetCharacterTemplate(characterClass);

        if (characterTemplate == null)
            return;

        InitializeStats(characterTemplate.parameters);
        InitializeAvailableWeapons(characterTemplate.weaponsCount, characterTemplate.weaponParameters, characterTemplate.weaponSkills);

        int weaponsCount = reader.ReadInt32();

        for (int i = 0; i < weaponsCount || i < availableWeapons.Count; i++)
        {
            availableWeapons[i].Load(reader);
        }

        while (weaponsCount > availableWeapons.Count)
        {
            Weapon newWeapon = new();
            newWeapon.Load(reader);
            weaponsCount--;
        }

        equipment.Load(reader, this);

        int positiveTraitsCount = reader.ReadInt32();
        for (int i = 0; i < positiveTraitsCount; i++)
        {
            AddTrait(RecruitingController.Instance.GetPositiveTraitByID(reader.ReadInt32()));
        }

        int negativeTraitsCount = reader.ReadInt32();
        for (int i = 0; i < negativeTraitsCount; i++)
        {
            AddTrait(RecruitingController.Instance.GetNegativeTraitByID(reader.ReadInt32()));
        }

        RecalculateStats();
        currentHealth = reader.ReadInt32();
    }
}
