using UnityEngine;

public class BattleCharacter : MonoBehaviour
{
    public bool isActiveTurn = false;

    // Основные характеристики персонажа
    public CharacterClass characterClass;

    // Показатили HP
    public int maxHP;
    public int currentHP;
    // Показатели Защиты
    public int baseDEF;
    public int currentDEF;
    // Показатели Уклонения
    public int baseEVA;
    public int currentEVA;
    // Показатели Провокации
    public int basePROV;
    public int currentPROV;
    // Показатели Скорости
    public int baseSPD;
    public int currentSPD;
    
    // Для SP: вместо одного поля SP, вводим два: максимальное и текущее
    public int maxSP;
    public int currentSP;
    public int SPreg;
    public int SPmovecost;
    
    // Параметры из оружий
    public int currentSE1;
    public int currentSE2;

    public int currentATK1;
    public int currentATK2;

    public int currentACC1;
    public int currentACC2;

    public int currentCRIT1;
    public int currentCRIT2;
    
    // Выбранные подклассы
    public int[] selectedSubclassIndices;  // Ожидается 2 значения
    
    // Характеристики оружий (два оружия)
    public WeaponParameters[] weaponParameters;  // Массив из 2-х элементов
    
    // Наборы навыков для оружий
    public WeaponSkillSet[] weaponSkills;        // Массив из 2-х элементов
    
    // Выбранные индексы дополнительных навыков 
    public int[] selectedSkillIndices;           // Массив из 3-х элементов
    
    public void Init(CharacterRuntimeParameters runtimeParams)
{
    characterClass = runtimeParams.characterClass;

    maxHP = runtimeParams.maxHP;
    currentHP = runtimeParams.currentHP;

    baseDEF = runtimeParams.DEF;
    currentDEF = baseDEF;

    baseEVA = runtimeParams.EVA;
    currentEVA = baseEVA;

    basePROV = runtimeParams.PROV;
    currentPROV = basePROV;

    baseSPD = runtimeParams.SPD;
    currentSPD = baseSPD;

    // Параметры SP
    maxSP = runtimeParams.SP;
    currentSP = maxSP;
    SPreg = runtimeParams.SPreg;
    SPmovecost = runtimeParams.SPmovecost;

    // Предполагаем, что runtimeParams.weaponParameters заполнен и содержит по крайней мере два элемента.
    weaponParameters = runtimeParams.weaponParameters;
    if (weaponParameters != null && weaponParameters.Length >= 2)
    {
        currentSE1 = weaponParameters[0].SE;
        currentSE2 = weaponParameters[1].SE;

        currentATK1 = weaponParameters[0].ATK;
        currentATK2 = weaponParameters[1].ATK;

        currentACC1 = weaponParameters[0].ACC;
        currentACC2 = weaponParameters[1].ACC;

        currentCRIT1 = weaponParameters[0].CRIT;
        currentCRIT2 = weaponParameters[1].CRIT;
    }
    else
    {
        Debug.LogWarning("Недостаточно параметров оружия для установки специальной энергии.");
    }
    
    selectedSubclassIndices = (int[])runtimeParams.selectedSubclassIndices.Clone();
    selectedSkillIndices = (int[])runtimeParams.selectedSkillIndices.Clone();
    weaponSkills = runtimeParams.weaponSkills;

    Debug.LogFormat("BattleCharacter initialized: Class = {0}, HP = {1}/{2}", characterClass, currentHP, maxHP);
}

    // public void TakeDamage(int damage)
    // {
    //     int effectiveDamage = Mathf.Max(damage - currentDEF, 1);
    //     currentHP -= effectiveDamage;
    //     Debug.Log(gameObject.name + " получил урон: " + effectiveDamage + ". Текущее HP = " + currentHP);

    //     if (currentHP <= 0)
    //     {
    //         Die();
    //     }
    // }

    // private void Die()
    // {
    //     Debug.Log(gameObject.name + " погиб.");
    //     Destroy(gameObject);
    // }
}
