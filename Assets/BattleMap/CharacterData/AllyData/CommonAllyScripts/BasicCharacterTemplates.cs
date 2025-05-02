using UnityEngine;

public class BasicCharacterTemplates : ScriptableObject
{
    [Header("Основные данные персонажа")]
    [ReadOnly] public CharacterClass characterClass;

    [Header("Структура оружия и навыков")]
    [Tooltip("Количество оружий (и соответствующих наборов навыков) данного персонажа. Рекомендуется сделать его равным длине availableSubclasses.")]
    public int weaponsCount = 0;

    [Header("Параметры оружия")]
    [Tooltip("Массив параметров оружия. Каждый элемент соответствует набору характеристик для конкретного оружия (подкласса).")]
    public WeaponParameters[] weaponParameters;

    [Header("Наборы навыков для оружия")]
    [Tooltip("Массив наборов навыков, один набор для каждого оружия. Индекс в этом массиве соответствует индексам в availableSubclasses.")]
    public WeaponSkillSet[] weaponSkills;

    public CharacterParameters parameters;

    // Константа для стандартного количества навыков в наборе
    private const int defaultSkillCount = 5;

    public WeaponParameters GetWeaponParameters(int subclassIndex)
    {
        if (weaponParameters != null && subclassIndex >= 0 && subclassIndex < weaponParameters.Length)
            return weaponParameters[subclassIndex];
        return null;
    }

    public WeaponSkillSet GetWeaponSkillSet(int subclassIndex)
    {
        if (weaponSkills != null && subclassIndex >= 0 && subclassIndex < weaponSkills.Length)
            return weaponSkills[subclassIndex];
        return null;
    }


#if UNITY_EDITOR
    /// <summary>
    /// Виртуальный метод, возвращающий количество доступных подклассов.
    /// Каждый производный класс должен переопределить его.
    /// </summary>
    protected virtual int GetAvailableSubclassesCount()
    {
        return 0;
    }

    /// <summary>
    /// Общий метод синхронизации данных.
    /// Синхронизирует weaponsCount с количеством доступных подклассов и инициализирует массивы,
    /// если они отсутствуют или их длина не соответствует weaponsCount.
    /// </summary>
    protected void OnValidateCommon()
    {
        int subclassCount = GetAvailableSubclassesCount();
        if (subclassCount != weaponsCount)
        {
            weaponsCount = subclassCount;
        }

        if (weaponParameters == null || weaponParameters.Length != weaponsCount)
        {
            weaponParameters = new WeaponParameters[weaponsCount];
        }

        if (weaponSkills == null || weaponSkills.Length != weaponsCount)
        {
            weaponSkills = new WeaponSkillSet[weaponsCount];
        }

        for (int i = 0; i < weaponsCount; i++)
        {
            if (weaponSkills[i] == null)
                weaponSkills[i] = new WeaponSkillSet();
            if (weaponSkills[i].skills == null || weaponSkills[i].skills.Length != defaultSkillCount)
                weaponSkills[i].skills = new SkillParameters[defaultSkillCount];
        }
    }
#endif
}
