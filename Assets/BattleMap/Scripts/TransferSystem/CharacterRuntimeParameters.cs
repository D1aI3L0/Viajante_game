using System;
using UnityEngine;

[Serializable]
public class CharacterRuntimeParameters
{
    [Header("Классификация персонажа")]
    [Tooltip("Основной класс персонажа, тип задаётся через enum.")]
    public CharacterClass characterClass;

    [Header("Выбранные подклассы (индексы)")]
    [Tooltip("Массив из 2-х индексов: выбираются подклассы для оружия.")]
    public int[] selectedSubclassIndices = new int[2];

    [Header("Характеристики персонажа")]
    public int maxHP;
    public int currentHP;
    public int DEF;
    public int EVA;
    public int PROV;
    public int SPD;
    public int SP;
    public int SPreg;
    public int SPmovecost;

    [Header("Параметры оружия")]
    [Tooltip("Массив из 2-х наборов параметров оружия, выбранных по индексу подкласса.")]
    public WeaponParameters[] weaponParameters = new WeaponParameters[2];

    [Header("Наборы навыков для оружия")]
    [Tooltip("Массив из 2-х наборов навыков, соответствующих каждому оружию. Базовая атака всегда есть, а остальные выбираются (индексы начинаются с 1).")]
    public WeaponSkillSet[] weaponSkills = new WeaponSkillSet[2];

    [Header("Выбранные навыки (дополнительные)")]
    [Tooltip("Массив из 3-х индексов выбранных дополнительных навыков (базовая атака всегда имеет индекс 0).")]
    public int[] selectedSkillIndices = new int[3];
}
