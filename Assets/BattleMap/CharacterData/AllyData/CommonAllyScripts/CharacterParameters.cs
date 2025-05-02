using System;
using UnityEngine;

[Serializable]
public class CharacterParameters
{
    // Группа «Характеристики живучести»
    [Header("Характеристики живучести")]
    [Tooltip("Максимальное здоровье персонажа")]
    public int maxHP;
    [Tooltip("Начальное (текущее) здоровье персонажа")]
    public int currentHP;
    [Tooltip("Защита персонажа")]
    public int DEF;
    [Tooltip("Уклонение персонажа")]
    public int EVA;
    [Tooltip("Показатель проворства или устойчивости персонажа")]
    public int PROV;

    // Группа «Характеристики ходов»
    [Header("Характеристики ходов")]
    [Tooltip("Скорость персонажа (например, скорость передвижения или инициатива в бою)")]
    public int SPD;
    [Tooltip("Базовая стоимость хода (SP)")]
    public int SP;
    [Tooltip("Регенерация SP за ход")]
    public int SPreg;
    [Tooltip("Стоимость передвижения (в SP)")]
    public int SPmovecost;
}
