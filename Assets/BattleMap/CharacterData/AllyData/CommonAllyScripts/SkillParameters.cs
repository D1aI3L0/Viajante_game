using UnityEngine;

[System.Serializable]
public class SkillParameters
{
    [Header("Общие параметры навыка")]
    public string skillName;
    public string description;

    // Диапазон урона в процентах от базового урона оружия
    public int minDamagePercentage;
    public int maxDamagePercentage;

    // Параметры точности навыка (например, % от базовой меткости)
    public int accuracyPercentage;

    // Расход ресурсов
    public int spCost;
    public int seCost;

    // Кулдаун навыка в ходах
    public int cooldownTurns;

    // Параметры, связанные с критическим уроном
    public int criticalRatePercentage;
    public float criticalHitMultiplier;
}
