using UnityEngine;

public enum Pathfinder
{
    LongBow,   // "Длинный лук"
    SmallCrossbowAndTraps    // "Малый арбалет и ловушки"
}

[CreateAssetMenu(fileName = "CharacterData_Pathfinder", menuName = "Characters/Следопыт Data", order = 1)]
public class CharacterData_Pathfinder : ScriptableObject
{
    [Header("Классификация персонажа")]
    [ReadOnly]public CharacterClass characterClass = CharacterClass.Pathfinder;
    
    [Header("Доступные подклассы")]
    [Tooltip("Список допустимых подклассов для данного класса. Например, индекс 0 – \"Длинный лук\", индекс 1 – \"Малый арбалет и ловушки\".")]
    [ReadOnly]public Pathfinder[] availableSubclasses = new Pathfinder[] 
    { 
        Pathfinder.LongBow, 
        Pathfinder.SmallCrossbowAndTraps 
    };

    [Header("Параметры персонажа")]
    public CharacterParameters parameters = new CharacterParameters();

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
        // Если количество оружий должно совпадать с количеством доступных подклассов, можно принудительно синхронизировать:
        if (availableSubclasses != null && availableSubclasses.Length != weaponsCount)
        {
            weaponsCount = availableSubclasses.Length;
        }

        // Инициализируем массив параметров оружия до длины weaponsCount.
        if (weaponParameters == null || weaponParameters.Length != weaponsCount)
        {
            weaponParameters = new WeaponParameters[weaponsCount];
        }

        // Инициализируем массив наборов навыков до длины weaponsCount.
        if (weaponSkills == null || weaponSkills.Length != weaponsCount)
        {
            weaponSkills = new WeaponSkillSet[weaponsCount];
        }

        // Для каждого набора навыков инициализируем сам массив навыков (например, стандартное количество – 5 навыков).
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
