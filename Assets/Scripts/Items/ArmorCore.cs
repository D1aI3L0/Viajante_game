using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ArmorCore : Item, IUpgradable
{
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum HiddenType
    {
        Balanced,
        HealthFocused,
        DefenceFocused,
        EvasionFocused
    }

    public Rarity rarity;
    public HiddenType hiddenType;

    [SerializeField]
    private List<ArmorCoreUpgrade> upgrades = new();

    public const int xUpgradeMax = 15;
    public const int yUpgradeMax = 15;

    public float HealthBonus { get; private set; }
    public float DefenceBonus { get; private set; }
    public float EvasionBonus { get; private set; }

    private static readonly Dictionary<Rarity, int> BurnThresholds = new Dictionary<Rarity, int>()
    {
        { Rarity.Common, 5 },
        { Rarity.Uncommon, 7 },
        { Rarity.Rare, 10 },
        { Rarity.Epic, 12 },
        { Rarity.Legendary, 15 }
    };

    private static readonly Dictionary<Rarity, float> BurnChances = new Dictionary<Rarity, float>()
    {
        { Rarity.Common, 0.3f },
        { Rarity.Uncommon, 0.2f },
        { Rarity.Rare, 0.15f },
        { Rarity.Epic, 0.1f },
        { Rarity.Legendary, 0.05f }
    };

    private const float burnChance = 0.5f;

    private static readonly Dictionary<HiddenType, Dictionary<SurvivalStatType, float>> StatProbabilities =
        new Dictionary<HiddenType, Dictionary<SurvivalStatType, float>>()
    {
        {
            HiddenType.Balanced, new Dictionary<SurvivalStatType, float>()
            {
                { SurvivalStatType.Balanced, 0.01f },
                { SurvivalStatType.Health, 0.33f },
                { SurvivalStatType.Defence, 0.33f },
                { SurvivalStatType.Evasion, 0.33f }
            }
        },
        {
            HiddenType.HealthFocused, new Dictionary<SurvivalStatType, float>()
            {
                { SurvivalStatType.Balanced, 0.1f },
                { SurvivalStatType.Health, 0.6f },
                { SurvivalStatType.Defence, 0.175f },
                { SurvivalStatType.Evasion, 0.175f }
            }
        },
        {
            HiddenType.DefenceFocused, new Dictionary<SurvivalStatType, float>()
            {
                { SurvivalStatType.Balanced, 0.05f },
                { SurvivalStatType.Health, 0.175f },
                { SurvivalStatType.Defence, 0.6f },
                { SurvivalStatType.Evasion, 0.175f }
            }
        },
        {
            HiddenType.EvasionFocused, new Dictionary<SurvivalStatType, float>()
            {
                { SurvivalStatType.Balanced, 0.05f },
                { SurvivalStatType.Health, 0.175f },
                { SurvivalStatType.Defence, 0.175f },
                { SurvivalStatType.Evasion, 0.6f }
            }
        }
    };

    public void Initialize()
    {
        hiddenType = HiddenType.Balanced;
        var centerUpgrade = new ArmorCoreUpgrade()
        {
            gridPosition = Vector2Int.zero,
            statType = SurvivalStatType.Balanced,
            bonusValue = 0,
            isBurned = false
        };

        upgrades.Add(centerUpgrade);
        CurrentLevel = 1;
        CalculateBonuses();
    }

    public bool TryUpgrade(out ArmorCoreUpgrade newUpgrade)
    {
        newUpgrade = null;

        var possiblePositions = GetPossibleUpgradePositions();

        if (possiblePositions.Count == 0)
            return false;

        var randomPosition = possiblePositions[UnityEngine.Random.Range(0, possiblePositions.Count)];

        var statType = ChooseRandomStatType();

        int bonusValue = CalculateBonusValue();

        bool isBurned = CheckForBurn();

        newUpgrade = new ArmorCoreUpgrade()
        {
            gridPosition = randomPosition,
            statType = statType,
            bonusValue = bonusValue,
            isBurned = isBurned
        };

        upgrades.Add(newUpgrade);
        CurrentLevel++;
        AddNewBonuses(newUpgrade);

        return true;
    }

    private List<Vector2Int> GetPossibleUpgradePositions()
    {
        var possiblePositions = new List<Vector2Int>();
        var occupiedPositions = new HashSet<Vector2Int>();

        foreach (var upgrade in upgrades)
        {
            if (upgrade.isBurned) continue;

            occupiedPositions.Add(upgrade.gridPosition);

            var neighborPositions = upgrade.GetHexNeighbors();

            foreach (var neighborPosition in neighborPositions)
            {
                if (!occupiedPositions.Contains(neighborPosition) && !possiblePositions.Contains(neighborPosition) && CheckPosition(neighborPosition))
                {
                    possiblePositions.Add(neighborPosition);
                }
            }
        }

        return possiblePositions;
    }

    private bool CheckPosition(Vector2Int position)
    {
        foreach (ArmorCoreUpgrade upgrade in upgrades)
        {
            if (upgrade.gridPosition.Equals(position) || position.x > xUpgradeMax || position.x < -xUpgradeMax || position.y > yUpgradeMax || position.y < -yUpgradeMax)
                return false;
        }
        return true;
    }

    private SurvivalStatType ChooseRandomStatType()
    {
        float randomValue = UnityEngine.Random.value;
        float cumulative = 0f;
        var probabilities = StatProbabilities[hiddenType];

        foreach (var kvp in probabilities)
        {
            cumulative += kvp.Value;
            if (randomValue <= cumulative)
                return kvp.Key;
        }

        return SurvivalStatType.Balanced;
    }

    private int CalculateBonusValue()
    {
        int rarityBonus = (int)rarity;
        int randomBonus = UnityEngine.Random.Range(1, 4);

        return rarityBonus + randomBonus;
    }

    private bool CheckForBurn()
    {
        if (CurrentLevel < BurnThresholds[rarity])
            return false;

        return UnityEngine.Random.value <= (burnChance * (CurrentLevel / BurnThresholds[rarity]));
    }

    private void CalculateBonuses()
    {
        HealthBonus = 0;
        DefenceBonus = 0;
        EvasionBonus = 0;

        foreach (var upgrade in upgrades)
        {
            if (upgrade.isBurned) continue;

            switch (upgrade.statType)
            {
                case SurvivalStatType.Health:
                    HealthBonus += upgrade.bonusValue;
                    break;
                case SurvivalStatType.Defence:
                    DefenceBonus += upgrade.bonusValue;
                    break;
                case SurvivalStatType.Evasion:
                    EvasionBonus += upgrade.bonusValue;
                    break;
                default:
                    HealthBonus += upgrade.bonusValue;
                    DefenceBonus += upgrade.bonusValue;
                    EvasionBonus += upgrade.bonusValue;
                    break;
            }
        }
    }

    private void AddNewBonuses(ArmorCoreUpgrade upgrade)
    {
        if (upgrade.isBurned) return;

        switch (upgrade.statType)
        {
            case SurvivalStatType.Health:
                HealthBonus += upgrade.bonusValue;
                break;
            case SurvivalStatType.Defence:
                DefenceBonus += upgrade.bonusValue;
                break;
            case SurvivalStatType.Evasion:
                EvasionBonus += upgrade.bonusValue;
                break;
            default:
                HealthBonus += upgrade.bonusValue;
                DefenceBonus += upgrade.bonusValue;
                EvasionBonus += upgrade.bonusValue;
                break;
        }
    }

    public List<ArmorCoreUpgrade> GetUpgrades()
    {
        return new List<ArmorCoreUpgrade>(upgrades);
    }
    //=================================================================================================
    //                                      IUpgradable
    //=================================================================================================
    public int CurrentLevel { get; private set; }

    public int GetUpgradeCost()
    {
        return CurrentLevel * 10 * ((int)rarity + 1);
    }

    public void Upgrade(out Upgrade upgrade)
    {
        TryUpgrade(out ArmorCoreUpgrade newUpgrade);
        upgrade = newUpgrade;
    }

    public string GetUpgradeDescription()
    {
        return $"Уровень: {CurrentLevel}\n" +
               $"Редкость: {rarity}\n" +
               $"Тип: {hiddenType}\n" +
               $"Бонусы:\n" +
               $"Здоровье: +{HealthBonus}%\n" +
               $"Защита: +{DefenceBonus}%\n" +
               $"Уклонение: +{EvasionBonus}%";
    }
}