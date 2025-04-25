using UnityEngine;

public class BasicCharacterTemplates : ScriptableObject
{
    [Header("Основные данные персонажа")]
    [ReadOnly] public CharacterClass characterClass;

    // Можно здесь разместить общие параметры персонажа,
    // которые будут у всех наследников.

    public CharacterParameters parameters;
}
