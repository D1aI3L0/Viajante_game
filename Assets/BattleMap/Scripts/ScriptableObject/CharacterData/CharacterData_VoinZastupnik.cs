using UnityEngine;

/// <summary>
/// Перечисление доступных подклассов для воина‑заступника.
/// </summary>
public enum WarriorSubclass
{
    ShieldAndMace,   // "Щит и булава"
    TwoHandedMace    // "Двуручная булава"
}

[CreateAssetMenu(fileName = "CharacterData_VoinZastupnik", menuName = "Characters/Воин-заступник Data", order = 1)]
public class CharacterData_VoinZastupnik : ScriptableObject
{
    [Header("Классификация персонажа")]
    [ReadOnly]public CharacterClass characterClass = CharacterClass.WarriorZastupnik;
    
    [Header("Доступные подклассы")]
    [Tooltip("Список допустимых подклассов для данного класса. Например, индекс 0 – \"Щит и булава\", индекс 1 – \"Двуручная булава\".")]
    public WarriorSubclass[] availableSubclasses = new WarriorSubclass[] { WarriorSubclass.ShieldAndMace, WarriorSubclass.TwoHandedMace };

    [Header("Характеристики живучести")]
    [Tooltip("Максимальное здоровье персонажа")]
    public int maxHP = 180;
    
    [Tooltip("Начальное (текущее) здоровье персонажа (при инициализации runtime это значение будет копировано из maxHP)")]
    public int currentHP = 180;
    public int DEF = 45;
    public int EVA = 12;
    public int PROV = 15;

    [Header("Характеристики ходов")]
    public int SPD = 120;
    public int SP = 100;
    public int SPreg = 15;
    public int SPmovecost = 5;

    [Header("Параметры оружия 1 (Shield and Mace)")]
    [Tooltip("Параметры для оружия, используемого при выборе ShieldAndMace")]
    public int DMG1 = 40;
    public int ACC1 = 95;
    public int CRIT1 = 10;
    public int SE1 = 100;
    public int SEreg1 = 15;
    public int SEdec1 = 0;

    [Header("Параметры оружия 2 (TwoHandedMace)")]
    [Tooltip("Параметры для оружия, используемого при выборе TwoHandedMace")]
    public int DMG2 = 45;
    public int ACC2 = 90;
    public int CRIT2 = 12;
    public int SE2 = 100;
    public int SEreg2 = 15;
    public int SEdec2 = 0;

    [Header("Навыки для оружия 1 (Shield and Mace)")]
    [Tooltip("Набор навыков для выбранного подкласса ShieldAndMace")]
    public SkillParameters[] weapon1Skills = new SkillParameters[5];

    [Header("Навыки для оружия 2 (TwoHandedMace)")]
    [Tooltip("Набор навыков для выбранного подкласса TwoHandedMace")]
    public SkillParameters[] weapon2Skills = new SkillParameters[5];

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Обеспечиваем корректную инициализацию массивов навыков.
        if (weapon1Skills == null || weapon1Skills.Length != 5)
            weapon1Skills = new SkillParameters[5];
        if (weapon2Skills == null || weapon2Skills.Length != 5)
            weapon2Skills = new SkillParameters[5];

        // --- Навыки для оружия 1: "Щит и булава" ---

        // [0] Базовая атака
        if (string.IsNullOrEmpty(weapon1Skills[0]?.skillName))
        {
            weapon1Skills[0] = new SkillParameters()
            {
                skillName = "Базовая атака",
                description = "Наносит 80%-95% от урона оружия одному противнику с 90% меткостью. Расходует SP 5 и не имеет перезарядки.",
                minDamagePercentage = 80,
                maxDamagePercentage = 95,
                accuracyPercentage = 90,
                spCost = 5,
                seCost = 0,
                cooldownTurns = 0,
                criticalRatePercentage = 0,
                criticalHitMultiplier = 1.0f
            };
        }

        // [1] Атака в рывке
        if (string.IsNullOrEmpty(weapon1Skills[1]?.skillName))
        {
            weapon1Skills[1] = new SkillParameters()
            {
                skillName = "Атака в рывке",
                description = "Совершает рывок к противнику (во время применения SPmovecost уменьшается на 2) и наносит 100%-110% от урона оружия одному противнику с 90% меткостью. Расходует SP 5 (плюс дополнительная стоимость за дистанцию) и SE 20, кулдаун — 2 хода.",
                minDamagePercentage = 100,
                maxDamagePercentage = 110,
                accuracyPercentage = 90,
                spCost = 5, // дополнительная логика может учитывать затрату для прохождения дистанции
                seCost = 20,
                cooldownTurns = 2,
                criticalRatePercentage = 0,
                criticalHitMultiplier = 1.0f
            };
        }

        // [2] Провокация
        if (string.IsNullOrEmpty(weapon1Skills[2]?.skillName))
        {
            weapon1Skills[2] = new SkillParameters()
            {
                skillName = "Провокация",
                description = "Повышает DEF на 15% на 2 хода и заставляет противника атаковать вас, если это возможно. Расходует SP 10 и SE 20, кулдаун — 2 хода.",
                minDamagePercentage = 0,
                maxDamagePercentage = 0,
                accuracyPercentage = 0,
                spCost = 10,
                seCost = 20,
                cooldownTurns = 2,
                criticalRatePercentage = 0,
                criticalHitMultiplier = 1.0f
            };
        }

        // [3] Контратака
        if (string.IsNullOrEmpty(weapon1Skills[3]?.skillName))
        {
            weapon1Skills[3] = new SkillParameters()
            {
                skillName = "Контратака",
                description = "Навык-активатор, который активируется и поддерживается до получения ближнего удара. При срабатывании наносит 100%-125% от урона оружия с 100% меткостью. Расходует SE 7 каждый ход, пока активен, и не имеет кулдауна.",
                minDamagePercentage = 100,
                maxDamagePercentage = 125,
                accuracyPercentage = 100,
                spCost = 0,
                seCost = 7, // расход SE каждый ход; обработка производится в логике
                cooldownTurns = 0,
                criticalRatePercentage = 0,
                criticalHitMultiplier = 1.0f
            };
        }

        // [4] Удар щитом
        if (string.IsNullOrEmpty(weapon1Skills[4]?.skillName))
        {
            weapon1Skills[4] = new SkillParameters()
            {
                skillName = "Удар щитом",
                description = "Проводит атаку по любой соседней клетке, а также затрагивает смежные клетки. Наносит 40%-80% от урона оружия с 85% меткостью, отталкивая противников на одну клетку в противоположном направлении. Расходует SP 12 и SE 15, кулдаун — 2 хода.",
                minDamagePercentage = 40,
                maxDamagePercentage = 80,
                accuracyPercentage = 85,
                spCost = 12,
                seCost = 15,
                cooldownTurns = 2,
                criticalRatePercentage = 0,
                criticalHitMultiplier = 1.0f
            };
        }

        // --- Навыки для оружия 2: "Двуручная булава" ---

        // [0] Базовая атака
        if (string.IsNullOrEmpty(weapon2Skills[0]?.skillName))
        {
            weapon2Skills[0] = new SkillParameters()
            {
                skillName = "Базовая атака",
                description = "Наносит 100%-110% от урона оружия одному противнику с 95% меткостью. Расходует SP 5 и не имеет перезарядки.",
                minDamagePercentage = 100,
                maxDamagePercentage = 110,
                accuracyPercentage = 95,
                spCost = 5,
                seCost = 0,
                cooldownTurns = 0,
                criticalRatePercentage = 0,
                criticalHitMultiplier = 1.0f
            };
        }

        // [1] Широкий взмах
        if (string.IsNullOrEmpty(weapon2Skills[1]?.skillName))
        {
            weapon2Skills[1] = new SkillParameters()
            {
                skillName = "Широкий взмах",
                description = "Проводит атаку по любой соседней клетке с затрагиванием также соседних ячеек. Наносит 90%-115% от урона оружия с 90% меткостью. Расходует SP 10 и SE 20, кулдаун — 2 хода.",
                minDamagePercentage = 90,
                maxDamagePercentage = 115,
                accuracyPercentage = 90,
                spCost = 10,
                seCost = 20,
                cooldownTurns = 2,
                criticalRatePercentage = 0,
                criticalHitMultiplier = 1.0f
            };
        }

        // [2] Удар навершием
        if (string.IsNullOrEmpty(weapon2Skills[2]?.skillName))
        {
            weapon2Skills[2] = new SkillParameters()
            {
                skillName = "Удар навершием",
                description = "Наносит 110%-130% от урона оружия одному противнику с 95% меткостью, отталкивая его на 3 клетки. Если противник сталкивается с препятствием, он оглушается на 1 ход. Расходует SP 15 и SE 30, кулдаун — 3 хода.",
                minDamagePercentage = 110,
                maxDamagePercentage = 130,
                accuracyPercentage = 95,
                spCost = 15,
                seCost = 30,
                cooldownTurns = 3,
                criticalRatePercentage = 0,
                criticalHitMultiplier = 1.0f
            };
        }

        // [3] Давящее присутствие (Пассивный навык)
        if (string.IsNullOrEmpty(weapon2Skills[3]?.skillName))
        {
            weapon2Skills[3] = new SkillParameters()
            {
                skillName = "Давящее присутствие",
                description = "Пассивный навык, действующий, пока персонаж находится в данной стойке. В радиусе 2 клеток противники получают снижение атаки на 20% и увеличение SPmovecost на 2 единицы.",
                minDamagePercentage = 0,
                maxDamagePercentage = 0,
                accuracyPercentage = 0,
                spCost = 0,
                seCost = 0,
                cooldownTurns = 0,
                criticalRatePercentage = 0,
                criticalHitMultiplier = 1.0f
            };
        }

        // [4] Последний удар
        if (string.IsNullOrEmpty(weapon2Skills[4]?.skillName))
        {
            weapon2Skills[4] = new SkillParameters()
            {
                skillName = "Последний удар",
                description = "Наносит 100%-110% от урона оружия плюс бонус 10% за каждые 10% дополнительного использованного SE, одному противнику с 95% меткостью. Повышает шанс критического удара (базовый крит оружия плюс бонус от навыка). Расходует SP 10 и SE равное сумме 20 плюс оставшаяся особая энергия, кулдаун — 4 хода.",
                minDamagePercentage = 100,
                maxDamagePercentage = 110,
                accuracyPercentage = 95,
                spCost = 10,
                seCost = 20, // логика может учитывать дополнительно оставшуюся энергию
                cooldownTurns = 4,
                criticalRatePercentage = 10, // примерное значение для повышения шанса крита
                criticalHitMultiplier = 1.0f  // стандартный множитель можно изменить при необходимости
            };
        }
    }
#endif
}
