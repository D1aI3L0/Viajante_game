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
    public PlayerCharacter[] playerCharacterData = new PlayerCharacter[0];

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Если массив runtime-параметров не создан или его длина меньше требуемой, пересоздаем его.
        characters = new CharacterRuntimeParameters[numberOfCharacters];

        if (playerCharacterData.Length > 0)
        {
            // Проходим по количеству персонажей, заданному в numberOfCharacters.
            for (int i = 0; i < numberOfCharacters; i++)
            {
                if (playerCharacterData[i] != null)
                {
                    if (characters[i] == null)
                        characters[i] = new CharacterRuntimeParameters();

                    // Копирование общих данных из базового ассета.
                    characters[i].characterClass = playerCharacterData[i].characterClass;
                    characters[i].maxHP = playerCharacterData[i].currentCharacterStats.maxHealth;
                    characters[i].currentHP = playerCharacterData[i].currentHealth;
                    characters[i].DEF = playerCharacterData[i].currentCharacterStats.defence;
                    characters[i].EVA = playerCharacterData[i].currentCharacterStats.evasion;
                    characters[i].PROV = playerCharacterData[i].currentCharacterStats.tount;
                    characters[i].SPD = playerCharacterData[i].currentCharacterStats.speed;
                    characters[i].SP = playerCharacterData[i].currentCharacterStats.SPamount;
                    characters[i].SPreg = playerCharacterData[i].currentCharacterStats.SPregen;
                    characters[i].SPmovecost = playerCharacterData[i].currentCharacterStats.SPmoveCost;

                    // Обеспечиваем, что для каждого персонажа выбрано ровно 2 подкласса (индексы выбора).
                    if (characters[i].selectedSubclassIndices == null || characters[i].selectedSubclassIndices.Length != 2)
                    {
                        characters[i].selectedSubclassIndices = new int[2] { 0, 1 };
                    }

                    // Обеспечиваем, что для каждого персонажа задан набор выбранных навыков для оружия
                    if (characters[i].weaponSkillSelections == null || characters[i].weaponSkillSelections.Length != 2)
                    {
                        characters[i].weaponSkillSelections = new WeaponSkillSelection[2];
                        for (int j = 0; j < 2; j++)
                        {
                            if (characters[i].weaponSkillSelections[j] == null)
                                characters[i].weaponSkillSelections[j] = new WeaponSkillSelection();
                        }
                    }


                    // Обработка параметров оружия и наборов навыков.
                    // Предполагаем, что для персонажа используется два оружия.
                    characters[i].weaponParameters = new WeaponParameters[2];
                    characters[i].weaponSkills = new WeaponSkillSet[2];

                    for (int j = 0; j < 2; j++)
                    {
                        // Используем универсальные методы базового класса для извлечения параметров.
                        Weapon weapon = playerCharacterData[i].GetWeaponInEquipnentByID(j);
                        if (weapon != null)
                        {
                            characters[i].weaponParameters[j] = weapon.weaponParameters;
                            characters[i].weaponSkills[j] = weapon.skillSet;
                        }
                    }
                }
            }
        }
    }
#endif

}
