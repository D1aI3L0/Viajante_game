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

#if UNITY_EDITOR
    // Переопределяем метод для возврата длины availableSubclasses
    protected override int GetAvailableSubclassesCount()
    {
        return availableSubclasses != null ? availableSubclasses.Length : 0;
    }

    private void OnValidate()
    {
        if (characterClass != CharacterClass.Pathfinder)
        {
            characterClass = CharacterClass.Pathfinder;
        }

        OnValidateCommon();
    }
#endif
}
