using UnityEngine;

public class BattleEntity : MonoBehaviour
{
    // Параметры, связанные с ходом персонажа
    public bool isActiveTurn = false;
    public float turnGauge = 0f;

    public virtual bool IsEnemy => false; // По умолчанию юнит не является врагом.
    public virtual bool IsAlly => !IsEnemy; // Юнит считается союзником, если не является врагом.
    
    // Основные характеристики здоровья
    public int maxHP;
    public int currentHP;
    
    // Характеристики Защиты
    public int baseDEF;
    public int currentDEF;
    
    // Характеристики Уклонения
    public int baseEVA;
    public int currentEVA;
    
    // Характеристики Скорости
    public int baseSPD;
    public int currentSPD;

}
