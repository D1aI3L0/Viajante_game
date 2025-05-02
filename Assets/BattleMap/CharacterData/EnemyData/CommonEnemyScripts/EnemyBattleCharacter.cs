using UnityEngine;

public class EnemyBattleCharacter : BattleEntity
{
    public override bool IsEnemy => true;
    
    // Вражеские параметры, специфичные для них
    public int baseATK = 35;
    public int currentATK;
    
    public int baseACC = 100;
    public int currentACC;
    
    public int baseCRIT = 10;
    public int currentCRIT;

    public void Init()
    {
        // Общие параметры уже могут быть установлены в инспекторе или инициализироваться заранее
        currentHP = maxHP;
        currentDEF = baseDEF;
        currentEVA = baseEVA;
        currentSPD = baseSPD;
        
        currentATK = baseATK;
        currentACC = baseACC;
        currentCRIT = baseCRIT;
    }
}
