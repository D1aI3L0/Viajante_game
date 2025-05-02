using UnityEngine;

public class AllyBattleCharacter : BattleEntity
{
    
    // Дополнительное поле для класса персонажа
    public CharacterClass characterClass;

    // Параметры SP
    public int maxSP;
    public int currentSP;
    public int SPreg;
    public int SPmovecost;
    
    // Параметры из оружия (значения для 2-х оружий)
    public int currentSE1;
    public int currentSE2;
    
    public int currentATK1;
    public int currentATK2;
    
    public int currentACC1;
    public int currentACC2;
    
    public int currentCRIT1;
    public int currentCRIT2;
    
    // Выбранные подклассы и навыки
    public int[] selectedSubclassIndices;  // Ожидается 2 значения
    public WeaponParameters[] weaponParameters;  // Массив из 2-х элементов
    public WeaponSkillSet[] weaponSkills;        // Массив из 2-х элементов
    public int[] selectedSkillIndices;           // Массив из 3-х элементов

    public void Init(CharacterRuntimeParameters runtimeParams)
    {
        characterClass = runtimeParams.characterClass;
        
        // Инициализация HP
        maxHP = runtimeParams.maxHP;
        currentHP = runtimeParams.currentHP;
        
        // Инициализация DEF
        baseDEF = runtimeParams.DEF;
        currentDEF = baseDEF;
        
        // Инициализация EVA
        baseEVA = runtimeParams.EVA;
        currentEVA = baseEVA;
        
        // Инициализация SPD
        baseSPD = runtimeParams.SPD;
        currentSPD = baseSPD;
        
        // Инициализация SP
        maxSP = runtimeParams.SP;
        currentSP = maxSP;
        SPreg = runtimeParams.SPreg;
        SPmovecost = runtimeParams.SPmovecost;
        
        // Инициализация параметров оружия, если массив заполнен
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
        
        Debug.LogFormat("BattleCharacter initialized: Class = {0}", characterClass);
    }
}
