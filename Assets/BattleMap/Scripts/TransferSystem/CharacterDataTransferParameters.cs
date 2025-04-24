using UnityEngine;

/// <summary>
/// Перечисление основных классов персонажей.
/// Тот же enum используется и в базовом ассете CharacterData_«название класса».
/// </summary>
public enum CharacterClass
{
    WarriorZastupnik,
    Pathfinder,
    Mage
    // Добавьте другие классы по необходимости.
}

[CreateAssetMenu(fileName = "CharacterDataTransferParameters",
    menuName = "Characters/Character Data Transfer Parameters", order = 2)]
public class CharacterDataTransferParameters : ScriptableObject
{
    [Header("Общее количество персонажей в бою (от 1 до 3)")]
    [Range(1, 3)]
    public int numberOfCharacters = 3;

    [Header("Параметры персонажей (runtime)")]
    [Tooltip("Массив runtime-параметров для каждого персонажа. Если персонажей меньше 3, оставшиеся элементы игнорируются.")]
    public CharacterRuntimeParameters[] characters = new CharacterRuntimeParameters[3];

    [Header("Базовые данные персонажей (шаблоны)")]
    [Tooltip("Ссылки на базовые ассеты для сопоставления (например, CharacterData_VoinZastupnik и т.д.).")]
    public CharacterData_VoinZastupnik[] baseCharacterData = new CharacterData_VoinZastupnik[3];

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Инициализируем массив runtime-параметров, если он ещё не создан или его длина меньше требуемой
        if (characters == null || characters.Length < numberOfCharacters)
        {
            characters = new CharacterRuntimeParameters[3];
        }

        for (int i = 0; i < numberOfCharacters; i++)
        {
            if (baseCharacterData[i] != null)
            {
                if (characters[i] == null)
                    characters[i] = new CharacterRuntimeParameters();

                // Копируем класс персонажа из базового ассета
                characters[i].characterClass = baseCharacterData[i].characterClass;

                // Копируем базовые характеристики из CharacterParameters базового ассета
                characters[i].maxHP = baseCharacterData[i].parameters.maxHP;
                characters[i].currentHP = baseCharacterData[i].parameters.currentHP;
                characters[i].DEF = baseCharacterData[i].parameters.DEF;
                characters[i].EVA = baseCharacterData[i].parameters.EVA;
                characters[i].PROV = baseCharacterData[i].parameters.PROV;

                characters[i].SPD = baseCharacterData[i].parameters.SPD;
                characters[i].SP = baseCharacterData[i].parameters.SP;
                characters[i].SPreg = baseCharacterData[i].parameters.SPreg;
                characters[i].SPmovecost = baseCharacterData[i].parameters.SPmovecost;

                // Обеспечиваем, что для каждого персонажа выбрано ровно 2 подкласса (индексы выбора)
                if (characters[i].selectedSubclassIndices == null || characters[i].selectedSubclassIndices.Length != 2)
                {
                    characters[i].selectedSubclassIndices = new int[2] { 0, 0 };
                }

                // Аналогично – три выбранных навыка (дополнительных, так как базовая атака всегда фиксирована и имеет индекс 0)
                if (characters[i].selectedSkillIndices == null || characters[i].selectedSkillIndices.Length != 3)
                {
                    // Значения по умолчанию – например, все равны 1 (индексы начинаются с 1, так как 0 уже занято базовой атакой)
                    characters[i].selectedSkillIndices = new int[3] { 1, 1, 1 };
                }

                // Обработка параметров оружия:
                // В бою используется 2 оружия, поэтому создаём массив из 2-х элементов,
                // где для каждого оружия параметры берутся из weaponParameters массива базового ассета,
                // по соответствующему выбранному индексу подкласса.
                characters[i].weaponParameters = new WeaponParameters[2];
                for (int j = 0; j < 2; j++)
                {
                    int chosenSubclass = characters[i].selectedSubclassIndices[j];
                    if (baseCharacterData[i].weaponParameters != null &&
                        baseCharacterData[i].weaponParameters.Length > chosenSubclass)
                    {
                        characters[i].weaponParameters[j] = baseCharacterData[i].weaponParameters[chosenSubclass];
                    }
                }

                // Аналогично копируем наборы навыков для оружия
                characters[i].weaponSkills = new WeaponSkillSet[2];
                for (int j = 0; j < 2; j++)
                {
                    int chosenSubclass = characters[i].selectedSubclassIndices[j];
                    if (baseCharacterData[i].weaponSkills != null &&
                        baseCharacterData[i].weaponSkills.Length > chosenSubclass)
                    {
                        characters[i].weaponSkills[j] = baseCharacterData[i].weaponSkills[chosenSubclass];
                    }
                }
            }
        }
    }
#endif
}
