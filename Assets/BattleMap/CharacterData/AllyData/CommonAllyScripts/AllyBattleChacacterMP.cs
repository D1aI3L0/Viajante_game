using System.Collections.Generic;
using UnityEngine;

public class AllyBattleCharacterMP : BattleEntityMP
{
    // Дополнительное поле для класса персонажа
    public CharacterClass characterClass;

    // Индекс текущей стойки (текущего оружия)
    public int currentWeaponIndex;

    // Массив базовых параметров оружия (для двух стойк)
    public WeaponParameters[] weaponParameters;
    // Остальные данные о навыках и подклассах
    public int[] selectedSubclassIndices;  // Ожидается 2 значения
    public WeaponSkillSet[] weaponSkills;    // Массив из 2-х элементов
    public WeaponSkillSelection[] weaponSkillSelections;       // Массив из 2-х элементов

    // Массивы для текущех current параметров из оружия
    public int[] currentATKValues = new int[2];
    public int[] currentACCValues = new int[2];
    public int[] currentCRITValues = new int[2];
    public int[] currentSEValues = new int[2];


    /// <summary>
    /// Текущая специальная энергия для выбранной стойки
    /// </summary>
    public int CurrentSE
    {
        get { return currentSEValues[currentWeaponIndex]; }
        set
        {
            currentSEValues[currentWeaponIndex] = value;
            RaiseOnStatsChanged();
        }
    }


    /// <summary>
    /// Текущая атака для выбранной стойки
    /// </summary>
    public int CurrentATK
    {
        get { return currentATKValues[currentWeaponIndex]; }
        set { currentATKValues[currentWeaponIndex] = value; }
    }

    /// <summary>
    /// Текущая меткость для выбранной стойки
    /// </summary>
    public int CurrentACC
    {
        get { return currentACCValues[currentWeaponIndex]; }
        set { currentACCValues[currentWeaponIndex] = value; }
    }

    /// <summary>
    /// Текущий критический шанс для выбранной стойки
    /// </summary>
    public int CurrentCRIT
    {
        get { return currentCRITValues[currentWeaponIndex]; }
        set { currentCRITValues[currentWeaponIndex] = value; }
    }

    // Метод для установки текущей клетки
    public void SetCurrentCell(BattleCell cell)
    {
        currentCell = cell;
    }

    public void SwitchWeapon()
    {
        currentWeaponIndex = (currentWeaponIndex == 0) ? 1 : 0;
        Debug.Log($"{name} переключился на стойку {currentWeaponIndex}");
        // Если требуется, можно добавить дополнительные действия, например, пересчет характеристик.

        // Если предусмотрено уведомление об изменениях, можно вызвать событие OnStatsChanged из базового класса:
        //OnStatsChanged?.Invoke();
    }



    /// <summary>
    /// Инициализация персонажа по данным runtime.
    /// Остальная логика инициализации остается прежней.
    /// </summary>
    public void Init(CharacterRuntimeParameters runtimeParams)
    {
        characterClass = runtimeParams.characterClass;

        // Инициализация HP, DEF, EVA, SPD
        maxHP = runtimeParams.maxHP;
        CurrentHP = runtimeParams.currentHP;

        baseDEF = runtimeParams.DEF;
        currentDEF = baseDEF;

        baseEVA = runtimeParams.EVA;
        currentEVA = baseEVA;

        baseSPD = runtimeParams.SPD;
        currentSPD = baseSPD;

        // Инициализация SP и восстановление значений
        maxSP = runtimeParams.SP;
        CurrentSP = maxSP;
        SPreg = runtimeParams.SPreg;
        SPmovecost = runtimeParams.SPmovecost;

        // Сначала присвоить weaponParameters из runtimeParams
        weaponParameters = runtimeParams.weaponParameters;
        if (weaponParameters == null || weaponParameters.Length < 2)
        {
            Debug.LogWarning("Недостаточно параметров оружия для установки специализированных характеристик.");
        }
        else
        {
            // Инициализировать SE для каждой стойки
            currentSEValues[0] = weaponParameters[0].SE;
            currentSEValues[1] = weaponParameters[1].SE;
            // Инициализировать ATK для каждой стойки
            currentATKValues[0] = weaponParameters[0].ATK;
            currentATKValues[1] = weaponParameters[1].ATK;
            // Инициализировать ACC (меткость) для каждой стойки
            currentACCValues[0] = weaponParameters[0].ACC;
            currentACCValues[1] = weaponParameters[1].ACC;
            // Инициализировать CRIT для каждой стойки
            currentCRITValues[0] = weaponParameters[0].CRIT;
            currentCRITValues[1] = weaponParameters[1].CRIT;
        }

        // Инициализация остальных массивов
        selectedSubclassIndices = (int[])runtimeParams.selectedSubclassIndices.Clone();
        weaponSkillSelections = new WeaponSkillSelection[runtimeParams.weaponSkillSelections.Length];
        for (int i = 0; i < runtimeParams.weaponSkillSelections.Length; i++)
        {
            weaponSkillSelections[i] = runtimeParams.weaponSkillSelections[i];
        }

        weaponSkills = runtimeParams.weaponSkills;

        // Установить текущую стойку по умолчанию
        currentWeaponIndex = 0;

        Debug.LogFormat("BattleCharacter initialized: Class = {0}", characterClass);
    }

    public override int[] GetSkillIDs()
    {
        List<int> skillIDs = new List<int>();

        for (int i = 0; i < weaponSkills.Length; i++)
        {
            for (int j = 0; j < weaponSkillSelections[i].selectedSkillIndices.Length; i++)
            {
                if (weaponSkills[i].skills[weaponSkillSelections[i].selectedSkillIndices[j]] != null)
                    skillIDs.Add(weaponSkills[i].skills[weaponSkillSelections[i].selectedSkillIndices[j]].GetInstanceID());
            }
        }

        return skillIDs.ToArray();
    }
}
