using System;
using UnityEngine;

[Serializable]
public class WeaponParameters
{
    [Header("Параметры оружия")]
    [Tooltip("Урон оружия")]
    public int DMG;
    
    [Tooltip("Точность удара")]
    public int ACC;
    
    [Tooltip("Критический шанс")]
    public int CRIT;
    
    [Tooltip("Базовая энергия оружия (SE)")]
    public int SE;
    
    [Tooltip("Регенерация SE")]
    public int SEreg;
    
    [Tooltip("Уменьшение расхода SE")]
    public int SEdec;
}
