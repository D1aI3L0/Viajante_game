using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;



[Serializable]
public class AttackStats
{
    public int attack;
    public int accuracy;
    public int critRate;
}

public enum SpecialEnergyType
{
    Rage,
    Endurance,

}

[Serializable]
public struct SpecialEnergy
{
    public static Color[] SpecialEnergyColors = 
    {
        Color.blue,
        Color.cyan,
        Color.green,
        Color.yellow
    };
    private const int combinedCoefficient = 85;

    public SpecialEnergyType energyType;
    public int amount;
    public int regen;
    public int decrease;

    public static SpecialEnergy? GetCombinedEnergy(SpecialEnergy specialEnergy1, SpecialEnergy specialEnergy2)
    {
        if(specialEnergy1.energyType != specialEnergy2.energyType)
            return null;

        return new SpecialEnergy()
        { 
            energyType = specialEnergy1.energyType,
            amount = (specialEnergy1.amount + specialEnergy2.amount) * combinedCoefficient / 100,
            regen = (specialEnergy1.amount + specialEnergy2.amount) * combinedCoefficient / 100,
            decrease = (specialEnergy1.amount + specialEnergy2.amount) * combinedCoefficient / 100
        };
    }
}


[Serializable]
public class Weapon : Item, IUpgradable
{
    public AttackStats attackStats = new();
    public SpecialEnergy specialEnergy = new();

    public List<Skill> skills = new();
    public List<WeaponUpgrade> upgrades = new();
    private Dictionary<WeaponUpgradeSkill, List<WeaponUpgradeRune>> upgradesDictionary = new();

    public const int xUpgradeMax = 12;
    public const int yUpgradeMax = 12;

    public void Initialize(WeaponParameters weaponParameters, WeaponSkillSet weaponSkillSet)
    {
        CurrentLevel = 1;
        InitializeStats(weaponParameters);
        InitializeSkillSet(weaponSkillSet);
        InitializeSkillUpgrades();
    }

    private void InitializeStats(WeaponParameters weaponParameters)
    {
        attackStats.attack = weaponParameters.ATK;
        attackStats.accuracy = weaponParameters.ACC;
        attackStats.critRate = weaponParameters.CRIT;
        specialEnergy.amount = weaponParameters.SE;
        specialEnergy.regen = weaponParameters.SEreg;
        specialEnergy.decrease = weaponParameters.SEdec;
    }

    private void InitializeSkillSet(WeaponSkillSet weaponSkillSet)
    {

    }

    private void InitializeSkillUpgrades()
    {
        InitializeBaseAttackUpgrade();
        InitializeSkillUpgrade(xUpgradeMax / 3, xUpgradeMax * 2 / 3, -(yUpgradeMax / 3), yUpgradeMax / 3);
        InitializeSkillUpgrade(-(xUpgradeMax * 2 / 3), -(xUpgradeMax / 3), yUpgradeMax / 3, yUpgradeMax * 2 / 3);
        InitializeSkillUpgrade(-(xUpgradeMax * 2 / 3), -(xUpgradeMax / 3), -(yUpgradeMax / 3), -(yUpgradeMax * 2 / 3));
    }

    private void InitializeBaseAttackUpgrade()
    {
        WeaponUpgradeSkill baseAttackUpgrade = new()
        {
            isFixed = true,
            gridPosition = Vector2Int.zero,
            linkedSkill = null
        };
        upgradesDictionary.Add(baseAttackUpgrade, new());
        upgrades.Add(baseAttackUpgrade);
    }

    private void InitializeSkillUpgrade(int xMin, int xMax, int yMin, int yMax)
    {
        Vector2Int position;

        while (!CheckPosition(position = new(UnityEngine.Random.Range(xMin, xMax), UnityEngine.Random.Range(yMin, yMax)))) ;

        WeaponUpgradeSkill weaponUpgradeSkill = new()
        {
            isFixed = false,
            gridPosition = position,
            linkedSkill = null
        };
        upgradesDictionary.Add(weaponUpgradeSkill, new());
        upgrades.Add(weaponUpgradeSkill);
    }

    private bool TryUpgrade(out WeaponUpgradeRune newWeaponUpgrade)
    {
        int index = UnityEngine.Random.Range(0, upgradesDictionary.Keys.Count);
        WeaponUpgradeSkill selectedSkillUpgrade = upgradesDictionary.Keys.ToList()[index];
        return TryUpgrade(selectedSkillUpgrade, out newWeaponUpgrade);
    }

    private bool TryUpgrade(WeaponUpgradeSkill selectedSkillUpgrade, out WeaponUpgradeRune newWeaponUpgrade)
    {
        newWeaponUpgrade = null;
        upgradesDictionary.TryGetValue(selectedSkillUpgrade, out var selectedRuneUpgrades);

        List<WeaponUpgrade> upgradesForCheck = new()
        {
            selectedSkillUpgrade
        };
        upgradesForCheck.AddRange(selectedRuneUpgrades);

        var possiblePositions = GetPossibleUpgradePositions(upgradesForCheck);

        if (possiblePositions.Count <= 0)
            return false;

        var randomPosition = possiblePositions[UnityEngine.Random.Range(0, possiblePositions.Count)];

        newWeaponUpgrade = new()
        {
            gridPosition = randomPosition,
            linkedRune = null,
            isBurned = false,
        };

        selectedRuneUpgrades.Add(newWeaponUpgrade);
        upgrades.Add(newWeaponUpgrade);

        return true;
    }

    private List<Vector2Int> GetPossibleUpgradePositions(List<WeaponUpgrade> weaponUpgrades)
    {
        var possiblePositions = new List<Vector2Int>();
        var occupiedPositions = new HashSet<Vector2Int>();

        foreach (var upgrade in weaponUpgrades)
        {
            if (upgrade is WeaponUpgradeRune weaponUpgradeRune && weaponUpgradeRune.isBurned) continue;

            occupiedPositions.Add(upgrade.gridPosition);

            var neighbors = upgrade.GetHexNeighbors();

            foreach (var neighbor in neighbors)
            {
                if (!occupiedPositions.Contains(neighbor) && !possiblePositions.Contains(neighbor) && CheckPosition(neighbor))
                {
                    possiblePositions.Add(neighbor);
                }
            }
        }

        return possiblePositions;
    }

    private bool CheckPosition(Vector2Int position)
    {
        foreach (var upgrade in upgrades)
        {
            if (upgrade.gridPosition.Equals(position) || position.x > xUpgradeMax || position.x < -xUpgradeMax || position.y > yUpgradeMax || position.y < -yUpgradeMax)
                return false;
        }
        return true;
    }

    public void ResetUpgrades(WeaponUpgradeSkill weaponUpgradeSkill)
    {
        upgradesDictionary.TryGetValue(weaponUpgradeSkill, out var runeUpgrades);

        int upgradesCount = runeUpgrades.Count();

        foreach (var runeUpgrade in runeUpgrades)
        {
            if (runeUpgrade.linkedRune != null)
                Base.Instance.inventory.Add(runeUpgrade.linkedRune);
        }

        for (int i = 0; i < upgradesCount; i++)
        {
            upgrades.Remove(runeUpgrades[i]);
        }
        runeUpgrades.Clear();

        for (int i = 0; i < upgradesCount; i++)
        {
            TryUpgrade(weaponUpgradeSkill, out var newWeaponUpgrade);
        }

        Debug.Log("New: " + runeUpgrades.Count);
    }

    public Dictionary<WeaponUpgradeSkill, List<WeaponUpgradeRune>> GetUprades()
    {
        return upgradesDictionary;
    }

    public int GetSkillNumberByRuneUpgrade(WeaponUpgradeRune weaponUpgrade)
    {
        List<WeaponUpgradeSkill> dictionaryKeys = upgradesDictionary.Keys.ToList();
        for (int i = 0; i < dictionaryKeys.Count; i++)
        {
            upgradesDictionary.TryGetValue(dictionaryKeys[i], out var dictionaryValue);
            if (dictionaryValue != null && dictionaryValue.Contains(weaponUpgrade))
                return i;
        }
        return -1;
    }

    public List<Skill> GetAvailableSkills()
    {
        List<Skill> availableSkills = new();
        var linkedSkills = GetLinkedSkills();

        for (int i = 0; i < skills.Count; i++)
        {
            if (!linkedSkills.Contains(skills[i]))
            {
                availableSkills.Add(skills[i]);
            }
        }

        return availableSkills;
    }

    public List<Skill> GetLinkedSkills()
    {
        List<Skill> linkedSkills = new();
        var skillUpgrades = upgradesDictionary.Keys.ToList();
        for (int i = 0; i < skillUpgrades.Count; i++)
        {
            if (skillUpgrades[i].linkedSkill != null)
            {
                linkedSkills.Add(skillUpgrades[i].linkedSkill);
            }
        }
        return linkedSkills;
    }

    public Dictionary<Skill, List<Rune>> GetLinkedSkillRunes()
    {
        Dictionary<Skill, List<Rune>> result = new();

        foreach (var skillUpgrade in upgradesDictionary.Keys.ToList())
        {
            if (skillUpgrade.linkedSkill != null)
            {
                List<Rune> linkedRunes = new();

                if (upgradesDictionary.TryGetValue(skillUpgrade, out var weaponUpgradeRunes))
                {
                    foreach (var runeUpgrade in weaponUpgradeRunes)
                    {
                        if (FindRunePath(skillUpgrade, runeUpgrade))
                        {
                            linkedRunes.Add(runeUpgrade.linkedRune);
                        }
                    }
                }

                result.Add(skillUpgrade.linkedSkill, linkedRunes);
            }
        }

        return result;
    }

    private bool FindRunePath(WeaponUpgradeSkill weaponUpgradeSkill, WeaponUpgradeRune weaponUpgradeRune)
    {
        List<WeaponUpgrade> occupiedUpgrades = new();

        Queue<WeaponUpgrade> possibleUpgradesToPath = new();

        possibleUpgradesToPath.Enqueue(weaponUpgradeRune);

        while (possibleUpgradesToPath.Count > 0)
        {
            WeaponUpgrade current = possibleUpgradesToPath.Dequeue();

            if (current is WeaponUpgradeRune upgradeRune && upgradeRune.linkedRune != null)
            {
                foreach (var d in upgradeRune.linkedRune.runeConnections)
                {
                    if (TryGetUpgrade(weaponUpgradeSkill, upgradeRune.CalculatePosition(d), out WeaponUpgrade findedWeaponUpgrade))
                    {
                        if (findedWeaponUpgrade is WeaponUpgradeSkill upgradeSkill && upgradeSkill == weaponUpgradeSkill)
                            return true;

                        if (possibleUpgradesToPath.Contains(findedWeaponUpgrade) || occupiedUpgrades.Contains(findedWeaponUpgrade))
                        {
                            continue;
                        }

                        if (findedWeaponUpgrade is WeaponUpgradeRune findedUpgradeRune && findedUpgradeRune.linkedRune != null && findedUpgradeRune.linkedRune.runeConnections.Contains(d.Opposite()))
                        {
                            possibleUpgradesToPath.Enqueue(findedWeaponUpgrade);
                        }
                    }
                }
            }
            occupiedUpgrades.Add(current);
        }

        return false;
    }

    private bool TryGetUpgrade(WeaponUpgradeSkill weaponUpgradeSkill, Vector2Int position, out WeaponUpgrade weaponUpgrade)
    {
        weaponUpgrade = null;

        if (weaponUpgradeSkill.gridPosition.Equals(position))
        {
            weaponUpgrade = weaponUpgradeSkill;
            return true;
        }

        if (upgradesDictionary.TryGetValue(weaponUpgradeSkill, out var weaponUpgradeRunes))
        {
            foreach (WeaponUpgrade upgrade in weaponUpgradeRunes)
            {
                if (upgrade.gridPosition.Equals(position))
                {
                    weaponUpgrade = upgrade;
                    return true;
                }
            }
        }

        return false;
    }

    private bool TyrGetUpgrade(Vector2Int position, out WeaponUpgrade weaponUpgrade)
    {
        weaponUpgrade = null;
        foreach (WeaponUpgrade upgrade in upgrades)
        {
            if (upgrade.gridPosition.Equals(position))
            {
                weaponUpgrade = upgrade;
                return true;
            }
        }
        return false;
    }
    //=================================================================================================
    //                                      IUpgradable
    //=================================================================================================
    public int CurrentLevel { get; private set; }

    public int GetUpgradeCost()
    {
        return CurrentLevel * 10;
    }

    public void Upgrade(out Upgrade upgrade)
    {
        if (TryUpgrade(out WeaponUpgradeRune newUpgrade))
            CurrentLevel++;
        upgrade = newUpgrade;
    }

    public string GetUpgradeDescription()
    {
        return $"Уровень: {CurrentLevel}\n";
    }
}
