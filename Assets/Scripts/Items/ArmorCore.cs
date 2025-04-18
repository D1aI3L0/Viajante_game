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
    public int currentLevel;
    public int maxLevel;

    public int CurrentLevel => currentLevel;
    public int MaxLevel => maxLevel;

    public int GetUpgradeCost()
    {
        return currentLevel * 10 * ((int)rarity + 1);
    }
    
    public void Upgrade()
    {
        if (currentLevel >= maxLevel) return;
        
        if (TryUpgrade(out var newUpgrade))
        {
            
        }
    }
    
    public string GetUpgradeDescription()
    {
        return $"Уровень: {currentLevel}/{maxLevel}\n" +
               $"Редкость: {rarity}\n" +
               $"Тип: {hiddenType}\n" +
               $"Бонусы:\n" +
               $"Здоровье: +{HealthBonus}%\n" +
               $"Защита: +{DefenceBonus}%\n" +
               $"Уклонение: +{EvasionBonus}%";
    }
    
    [SerializeField] 
    private List<ArmorCoreUpgrade> upgrades = new List<ArmorCoreUpgrade>();
    
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

    private static readonly Dictionary<HiddenType, Dictionary<SurvivalStatType, float>> StatProbabilities = 
        new Dictionary<HiddenType, Dictionary<SurvivalStatType, float>>()
    {
        {
            HiddenType.Balanced, new Dictionary<SurvivalStatType, float>()
            {
                { SurvivalStatType.Health, 0.33f },
                { SurvivalStatType.Defence, 0.33f },
                { SurvivalStatType.Evasion, 0.34f }
            }
        },
        {
            HiddenType.HealthFocused, new Dictionary<SurvivalStatType, float>()
            {
                { SurvivalStatType.Health, 0.6f },
                { SurvivalStatType.Defence, 0.2f },
                { SurvivalStatType.Evasion, 0.2f }
            }
        },
        {
            HiddenType.DefenceFocused, new Dictionary<SurvivalStatType, float>()
            {
                { SurvivalStatType.Health, 0.2f },
                { SurvivalStatType.Defence, 0.6f },
                { SurvivalStatType.Evasion, 0.2f }
            }
        },
        {
            HiddenType.EvasionFocused, new Dictionary<SurvivalStatType, float>()
            {
                { SurvivalStatType.Health, 0.2f },
                { SurvivalStatType.Defence, 0.2f },
                { SurvivalStatType.Evasion, 0.6f }
            }
        }
    };

    public void Initialize()
    {
        var centerUpgrade = new ArmorCoreUpgrade()
        {
            gridPosition = Vector2Int.zero,
            statType = SurvivalStatType.Health,
            bonusValue = 0,
            isBurned = false
        };
        
        upgrades.Add(centerUpgrade);
        currentLevel = 1;
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
        currentLevel++;
        CalculateBonuses();
        
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
            
            var neighbors = GetHexNeighbors(upgrade.gridPosition);
            
            foreach (var neighbor in neighbors)
            {
                if (!occupiedPositions.Contains(neighbor) && !possiblePositions.Contains(neighbor))
                {
                    possiblePositions.Add(neighbor);
                }
            }
        }
        
        return possiblePositions;
    }

    private Vector2Int[] GetHexNeighbors(Vector2Int position)
    {
        return new Vector2Int[]
        {
            position + new Vector2Int(1, 0),
            position + new Vector2Int(1, -1),
            position + new Vector2Int(0, -1),
            position + new Vector2Int(-1, 0),
            position + new Vector2Int(-1, 1),
            position + new Vector2Int(0, 1)
        };
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
        
        return SurvivalStatType.Health;
    }

    private int CalculateBonusValue()
    {
        int rarityBonus = (int)rarity * 2;
        int randomBonus = UnityEngine.Random.Range(1, 4);
        
        return rarityBonus + randomBonus;
    }

    private bool CheckForBurn()
    {
        if (currentLevel < BurnThresholds[rarity])
            return false;
        
        return UnityEngine.Random.value <= BurnChances[rarity];
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
            }
        }
    }

    public List<ArmorCoreUpgrade> GetUpgrades()
    {
        return new List<ArmorCoreUpgrade>(upgrades);
    }
}