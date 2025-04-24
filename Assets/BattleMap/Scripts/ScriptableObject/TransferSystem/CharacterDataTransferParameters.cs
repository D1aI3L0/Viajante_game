using UnityEngine;

/// <summary>
/// Перечисление основных классов персонажей.
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

    [Header("Параметры персонажей")]
    [Tooltip("Массив параметров для каждого персонажа. Если персонажей меньше 3, оставшиеся элементы игнорируются.")]
    public CharacterRuntimeParameters[] characters = new CharacterRuntimeParameters[3];

    [Header("Базовые данные персонажей")]
    [Tooltip("Ссылки на базовые ассеты для сопоставления (например, CharacterData_VoinZastupnik и т.д.).")]
    public CharacterData_VoinZastupnik[] baseCharacterData = new CharacterData_VoinZastupnik[3];

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Обеспечиваем корректную инициализацию массива runtime-параметров.
        if (characters == null || characters.Length < numberOfCharacters)
        {
            characters = new CharacterRuntimeParameters[3];
        }

        // Синхронизируем базовые данные с runtime-параметрами.
        for (int i = 0; i < numberOfCharacters; i++)
        {
            if (baseCharacterData[i] != null)
            {
                if (characters[i] == null)
                {
                    characters[i] = new CharacterRuntimeParameters();
                }

                // Автоматически копируем тип класса.
                characters[i].characterClass = baseCharacterData[i].characterClass;

                // Здесь мы не копируем конкретные подклассы, а оставляем индексы для выбора.
                // Если индексы ещё не заданы или длина массива меньше требуемой, устанавливаем дефолтно нули.
                if(characters[i].selectedSubclassIndices == null || characters[i].selectedSubclassIndices.Length < 1)
                {
                    // Например, для двух слотов.
                    characters[i].selectedSubclassIndices = new int[2] { 0, 0 };
                }
            }
        }
    }
#endif
}

[System.Serializable]
public class CharacterRuntimeParameters
{
    [Header("Классификация персонажа")]
    [Tooltip("Основной класс персонажа, тип задаётся через enum.")]
    public CharacterClass characterClass;

    [Header("Выбранные подклассы (индексы)")]
    [Tooltip("Массив индексов выбранных подклассов из списка availableSubclasses базового ассета. Например, индекс 0 соответствует первому значению.")]
    public int[] selectedSubclassIndices = new int[2];

    [Header("Характеристики живучести")]
    public int maxHP;
    public int currentHP;
    public int DEF;
    public int EVA;
    public int PROV;

    [Header("Характеристики ходов")]
    public int SPD;
    public int SP;
    public int SPreg;
    public int SPmovecost;

    [Header("Параметры оружия 1")]
    public int DMG1;
    public int ACC1;
    public int CRIT1;
    public int SE1;
    public int SEreg1;
    public int SEdec1;

    [Header("Параметры оружия 2")]
    public int DMG2;
    public int ACC2;
    public int CRIT2;
    public int SE2;
    public int SEreg2;
    public int SEdec2;

    [Header("Выбранные навыки")]
    [Tooltip("Массив идентификаторов выбранных навыков. Базовая атака (индекс 0) всегда присутствует, а игрок может выбрать до 3 дополнительных навыков.")]
    public string[] selectedSkillIDs;
}
