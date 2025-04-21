using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Characters/Character Data", order = 1)]
public class CharacterData : ScriptableObject
{
    [Header("Основные параметры")]
    public string characterName;
    public float health;
    public float defense;
    public float evasion;
    // Параметры оружия 1
    public float attack1;
    public float accuracy1;
    public float critChance1;
    public float specialEnergy1;
    // Параметры оружия 2
    public float attack2;
    public float accuracy2;
    public float critChance2;
    public float specialEnergy2;

    public float initiative;

    [Header("SP и провокация")]
    public int sp;              // Очки действий
    public int spRecovery;      // Восстановление SP в ход
    public int spCost;          // Стоимость SP на ход (например, при перемещении)
    public int provocation;     // Провокация персонажа

    // навыки пока что закомментирован так как появляются ошибки
    //[Header("Навыки")] 
    // Здесь можно указать ссылки на навыки персонажа, если они тоже реализованы как ScriptableObject.

    //public SkillData[] skills; // пока что закомментирован так как скилы не реализованы
}
