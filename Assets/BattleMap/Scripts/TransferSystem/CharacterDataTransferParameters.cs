using UnityEngine;

/// <summary>
/// Перечисление основных классов персонажей.
/// Тот же enum используется и в базовом ассете BasicCharacterTemplates (а также в наследниках).
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
    [Tooltip("Ссылки на базовые ассеты для сопоставления (например, CharacterData_VoinZastupnik, CharacterData_Pathfinder и т.д.).")]
    public BasicCharacterTemplates[] baseCharacterData = new BasicCharacterTemplates[3];

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Если массив runtime-параметров не создан или его длина меньше требуемой, пересоздаем его.
        if (characters == null || characters.Length < numberOfCharacters)
        {
            characters = new CharacterRuntimeParameters[3];
        }

        // Проходим по количеству персонажей, заданному в numberOfCharacters.
        for (int i = 0; i < numberOfCharacters; i++)
        {
            if (baseCharacterData[i] != null)
            {
                if (characters[i] == null)
                    characters[i] = new CharacterRuntimeParameters();

                // Копирование общих данных из базового ассета.
                characters[i].characterClass = baseCharacterData[i].characterClass;
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

                // Аналогично – три выбранных навыка (дополнительных), так как базовая атака всегда имеет индекс 0.
                if (characters[i].selectedSkillIndices == null || characters[i].selectedSkillIndices.Length != 3)
                {
                    characters[i].selectedSkillIndices = new int[3] { 1, 1, 1 };
                }

                // Обработка параметров оружия и наборов навыков.
                // Здесь предполагается, что для персонажа используется два оружия.
                characters[i].weaponParameters = new WeaponParameters[2];
                characters[i].weaponSkills = new WeaponSkillSet[2];

                // В зависимости от конкретного типа базового ассета копируем оружейные данные.
                if (baseCharacterData[i] is CharacterData_VoinZastupnik warriorData)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int chosenSubclass = characters[i].selectedSubclassIndices[j];
                        if (warriorData.weaponParameters != null &&
                            warriorData.weaponParameters.Length > chosenSubclass)
                        {
                            characters[i].weaponParameters[j] = warriorData.weaponParameters[chosenSubclass];
                        }
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        int chosenSubclass = characters[i].selectedSubclassIndices[j];
                        if (warriorData.weaponSkills != null &&
                            warriorData.weaponSkills.Length > chosenSubclass)
                        {
                            characters[i].weaponSkills[j] = warriorData.weaponSkills[chosenSubclass];
                        }
                    }
                }
                else if (baseCharacterData[i] is CharacterData_Pathfinder pathfinderData)
                {
                    for (int j = 0; j < 2; j++)
                    {
                        int chosenSubclass = characters[i].selectedSubclassIndices[j];
                        if (pathfinderData.weaponParameters != null &&
                            pathfinderData.weaponParameters.Length > chosenSubclass)
                        {
                            characters[i].weaponParameters[j] = pathfinderData.weaponParameters[chosenSubclass];
                        }
                    }

                    for (int j = 0; j < 2; j++)
                    {
                        int chosenSubclass = characters[i].selectedSubclassIndices[j];
                        if (pathfinderData.weaponSkills != null &&
                            pathfinderData.weaponSkills.Length > chosenSubclass)
                        {
                            characters[i].weaponSkills[j] = pathfinderData.weaponSkills[chosenSubclass];
                        }
                    }
                }
            }
        }
    }
#endif
}
