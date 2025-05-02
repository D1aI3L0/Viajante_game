using UnityEngine;

public enum WarriorSubclass
{
    ShieldAndMace,   // "Щит и булава"
    TwoHandedMace    // "Двуручная булава"
}

[CreateAssetMenu(fileName = "CharacterData_VoinZastupnik", menuName = "Characters/Воин‑заступник Data", order = 1)]
public class CharacterData_VoinZastupnik : BasicCharacterTemplates
{
    [Header("Доступные подклассы")]
    [Tooltip("Список допустимых подклассов для данного класса. Например, индекс 0 – \"Щит и булава\", индекс 1 – \"Двуручная булава\".")]
    [ReadOnly]
    public WarriorSubclass[] availableSubclasses = new WarriorSubclass[]
    {
        WarriorSubclass.ShieldAndMace,
        WarriorSubclass.TwoHandedMace
    };

#if UNITY_EDITOR
    // Переопределяем метод для возврата длины availableSubclasses
    protected override int GetAvailableSubclassesCount()
    {
        return availableSubclasses != null ? availableSubclasses.Length : 0;
    }

    private void OnValidate()
    {
        // Гарантируем, что для этого ассета всегда устанавливается нужный класс.
        if (characterClass != CharacterClass.WarriorZastupnik)
        {
            characterClass = CharacterClass.WarriorZastupnik;
        }

        // Вызываем общий метод синхронизации из базового класса.
        OnValidateCommon();
    }
#endif
}
