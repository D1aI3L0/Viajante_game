using UnityEngine;

public class EnemyBattleCharacter : MonoBehaviour
{
    public bool isEnemyTurn = false;

    // Показатили HP
    public int maxHP = 200;
    public int currentHP;

    // Показатели Защиты
    public int baseDEF = 35;
    public int currentDEF;

    // Показатели Уклонения
    public int baseEVA = 15;
    public int currentEVA;

    // Показатели Скорости
    public int baseSPD = 130;
    public int currentSPD;

    // Показатели Атаки
    public int baseATK = 35;
    public int currentATK;

    // Показатели Меткости
    public int baseACC = 100;
    public int currentACC;

    // Показатели Крита
    public int baseCRIT = 10;
    public int currentCRIT;


    public void Init()
    {
        currentHP = maxHP;
        currentDEF = baseDEF;
        currentEVA = baseEVA;
        currentSPD = baseSPD;
        currentATK = baseATK;
        currentACC = baseACC;
        currentCRIT = baseCRIT;
    }

}
