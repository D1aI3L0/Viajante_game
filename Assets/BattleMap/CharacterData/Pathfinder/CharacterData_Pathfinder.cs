using UnityEngine;

public enum Pathfinder
{
    LongBow,               // "Длинный лук"
    SmallCrossbowAndTraps  // "Малый арбалет и ловушки"
}

[CreateAssetMenu(fileName = "CharacterData_Pathfinder", menuName = "Characters/Следопыт Data", order = 1)]
public class CharacterData_Pathfinder : BasicCharacterTemplates
{
    [Header("Доступные подклассы")]
    [Tooltip("Список допустимых подклассов для данного класса. Например, индекс 0 – \"Длинный лук\", индекс 1 – \"Малый арбалет и ловушки\".")]

    [ReadOnly]
    public Pathfinder[] availableSubclasses = new Pathfinder[]
    {
        Pathfinder.LongBow,
        Pathfinder.SmallCrossbowAndTraps
    };

    [Header("Структура оружия и навыков")]
    [Tooltip("Количество оружий (и соответствующих наборов навыков) данного персонажа. Рекомендуется сделать его равным длине availableSubclasses.")]
    public int weaponsCount = 2;

    [Header("Параметры оружия")]
    [Tooltip("Массив параметров оружия. Каждый элемент соответствует набору характеристик для конкретного оружия (подкласса).")]
    public WeaponParameters[] weaponParameters;

    [Header("Наборы навыков для оружия")]
    [Tooltip("Массив наборов навыков, один набор для каждого оружия. Индекс в этом массиве соответствует индексам в availableSubclasses.")]
    public WeaponSkillSet[] weaponSkills;

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Устанавливаем, что для этого ассета всегда используется класс Pathfinder.
        if (characterClass != CharacterClass.Pathfinder)
        {
            characterClass = CharacterClass.Pathfinder;
        }

        // Синхронизация количества оружий с количеством доступных подклассов.
        if (availableSubclasses != null && availableSubclasses.Length != weaponsCount)
        {
            weaponsCount = availableSubclasses.Length;
        }

        // Инициализация массива параметров оружия, если он не задан или его длина не соответствует.
        if (weaponParameters == null || weaponParameters.Length != weaponsCount)
        {
            weaponParameters = new WeaponParameters[weaponsCount];
        }

        // Инициализация массива наборов навыков, если он не задан или его длина не соответствует.
        if (weaponSkills == null || weaponSkills.Length != weaponsCount)
        {
            weaponSkills = new WeaponSkillSet[weaponsCount];
        }

        // Для каждого набора навыков инициализируем массив навыков (например, стандартное количество – 5 навыков).
        for (int i = 0; i < weaponsCount; i++)
        {
            if (weaponSkills[i] == null)
                weaponSkills[i] = new WeaponSkillSet();
            if (weaponSkills[i].skills == null || weaponSkills[i].skills.Length != 5)
                weaponSkills[i].skills = new SkillParameters[5];
        }
    }
#endif
}
