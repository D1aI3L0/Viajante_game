using UnityEngine;

public class AllyBattleCharacter : BattleEntity
{
    // Дополнительное поле для класса персонажа
    public CharacterClass characterClass;

    // Индекс текущей стойки (текущего оружия)
    public int currentWeaponIndex;
    
    // Массив базовых параметров оружия (для двух стойк)
    public WeaponParameters[] weaponParameters;
    // Остальные данные о навыках и подклассах
    public int[] selectedSubclassIndices;  // Ожидается 2 значения
    public WeaponSkillSet[] weaponSkills;    // Массив из 2-х элементов
    public WeaponSkillSelection[] weaponSkillSelections;       // Массив из 2-х элементов

    
    /// <summary>
    /// Текущая специальная энергия для выбранной стойки
    /// </summary>
    public int CurrentSE
    {
        get
        {
            if (weaponParameters != null && weaponParameters.Length > currentWeaponIndex)
                return weaponParameters[currentWeaponIndex].SE;  // Можно добавить модификаторы, если нужно
            return 0;
        }
    }

    /// <summary>
    /// Текущая атака для выбранной стойки
    /// </summary>
    public int CurrentATK
    {
        get
        {
            if (weaponParameters != null && weaponParameters.Length > currentWeaponIndex)
                return weaponParameters[currentWeaponIndex].ATK;
            return 0;
        }
    }

    /// <summary>
    /// Текущая меткость для выбранной стойки
    /// </summary>
    public int CurrentACC
    {
        get
        {
            if (weaponParameters != null && weaponParameters.Length > currentWeaponIndex)
                return weaponParameters[currentWeaponIndex].ACC;
            return 0;
        }
    }

    /// <summary>
    /// Текущий критический шанс для выбранной стойки
    /// </summary>
    public int CurrentCRIT
    {
        get
        {
            if (weaponParameters != null && weaponParameters.Length > currentWeaponIndex)
                return weaponParameters[currentWeaponIndex].CRIT;
            return 0;
        }
    }

    // Метод для установки текущей клетки
    public void SetCurrentCell(BattleCell cell)
    {
        currentCell = cell;
    }


    /// <summary>
    /// Инициализация персонажа по данным runtime.
    /// Остальная логика инициализации остается прежней.
    /// </summary>
    public void Init(CharacterRuntimeParameters runtimeParams)
    {
        characterClass = runtimeParams.characterClass;

        // Инициализация HP
        maxHP = runtimeParams.maxHP;
        CurrentHP = runtimeParams.currentHP;

        // Инициализация DEF
        baseDEF = runtimeParams.DEF;
        currentDEF = baseDEF;

        // Инициализация EVA
        baseEVA = runtimeParams.EVA;
        currentEVA = baseEVA;

        // Инициализация SPD
        baseSPD = runtimeParams.SPD;
        currentSPD = baseSPD;

        // Инициализация SP
        maxSP = runtimeParams.SP;
        CurrentSP = maxSP;
        SPreg = runtimeParams.SPreg;
        SPmovecost = runtimeParams.SPmovecost;

        // Инициализация параметров оружия, если массив заполнен
        weaponParameters = runtimeParams.weaponParameters;

        if (weaponParameters == null || weaponParameters.Length < 2)
        {
            Debug.LogWarning("Недостаточно параметров оружия для установки специализированных характеристик.");
        }

        selectedSubclassIndices = (int[])runtimeParams.selectedSubclassIndices.Clone();
        weaponSkillSelections = new WeaponSkillSelection[runtimeParams.weaponSkillSelections.Length];
        for (int i = 0; i < runtimeParams.weaponSkillSelections.Length; i++)
        {
            // Глубокое клонирование или копирование значений, если необходимо.
            // Например, можно просто присвоить ссылку, если это не вызывает нежелательного поведения:
            weaponSkillSelections[i] = runtimeParams.weaponSkillSelections[i];
        }

        weaponSkills = runtimeParams.weaponSkills;

        // Можно инициализировать currentWeaponIndex начальным значением, например 0:
        currentWeaponIndex = 0;

        Debug.LogFormat("BattleCharacter initialized: Class = {0}", characterClass);
    }
}
