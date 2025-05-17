using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;



[Serializable]
public class Weapon : Item, IUpgradable
{
    public WeaponParameters weaponParameters = new();

    public WeaponSkillSet skillSet;
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

    private void InitializeStats(WeaponParameters otherWeaponParameters)
    {
        weaponParameters.ATK = otherWeaponParameters.ATK;
        weaponParameters.ACC = otherWeaponParameters.ACC;
        weaponParameters.CRIT = otherWeaponParameters.CRIT;
        weaponParameters.SE = otherWeaponParameters.SE;
        weaponParameters.SEreg = otherWeaponParameters.SEreg;
        weaponParameters.SEdec = otherWeaponParameters.SEdec;
    }

    private void InitializeSkillSet(WeaponSkillSet weaponSkillSet)
    {
        skillSet = new(weaponSkillSet);
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
        };
        _ = skillSet.skills.Length > 1 ? baseAttackUpgrade.linkedSkill = skillSet.skills[0] : baseAttackUpgrade.linkedSkill = null;
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

    public bool TryUpgrade(out WeaponUpgradeRune newWeaponUpgrade)
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

    public List<SkillAsset> GetAvailableSkills()
    {
        List<SkillAsset> availableSkills = new();
        var linkedSkills = GetLinkedSkills();

        for (int i = 0; i < skillSet.skills.Length; i++)
        {
            if (!linkedSkills.Contains(skillSet.skills[i]))
            {
                availableSkills.Add(skillSet.skills[i]);
            }
        }

        return availableSkills;
    }

    public List<SkillAsset> GetLinkedSkills()
    {
        List<SkillAsset> linkedSkills = new();
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

    public Dictionary<SkillAsset, List<Rune>> GetLinkedSkillRunes()
    {
        Dictionary<SkillAsset, List<Rune>> result = new();

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

    public int GetSkillID(SkillAsset skill)
    {
        if (skill == null)
            return -1;

        for (int i = 0; i < skillSet.skills.Length; i++)
        {
            if (skillSet.skills[i] == skill)
                return i;
        }
        return -1;
    }

    public SkillAsset GetSkillByID(int id)
    {
        if (id < 0 || id > skillSet.skills.Length)
            return null;

        return skillSet.skills[id];
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
        return $"Level: {CurrentLevel}\n";
    }
    //=================================================================================================
    //                                      Сохранение и загрузка
    //=================================================================================================
    public override void Save(BinaryWriter writer)
    {
        base.Save(writer);
        writer.Write(CurrentLevel);

        var upgradeSkills = upgradesDictionary.Keys.ToList();
        for (int i = 0; i < 4; i++)  
        {
            upgradeSkills[i].Save(writer, this);

            upgradesDictionary.TryGetValue(upgradeSkills[i], out var upgradeRunes);
            writer.Write(upgradeRunes.Count);
            foreach (WeaponUpgradeRune upgradeRune in upgradeRunes)
            {
                upgradeRune.Save(writer);
            }
        }
    }

    public override void Load(BinaryReader reader)
    {
        base.Load(reader);
        upgrades.Clear();
        upgradesDictionary.Clear();

        CurrentLevel = reader.ReadInt32();

        for (int i = 0; i < 4; i++)
        {
            WeaponUpgradeSkill newSkillUpgrade = new();
            newSkillUpgrade.Load(reader, this);

            int runeUpgradeCount = reader.ReadInt32();
            List<WeaponUpgradeRune> newRuneUpgrades = new();
            for (int j = 0; j < runeUpgradeCount; j++)
            {
                WeaponUpgradeRune newRuneUpgrade = new();
                newRuneUpgrade.Load(reader);
                newRuneUpgrades.Add(newRuneUpgrade);
            }
            upgradesDictionary.Add(newSkillUpgrade, newRuneUpgrades);
            upgrades.Add(newSkillUpgrade);
            upgrades.AddRange(newRuneUpgrades);
        }
    }
}
