using System.Linq;
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

    public int currentSE, maxSE;
    public int currentSEreg, currentSEdec;

    public WeaponSkillSet skillSet;

    public void Init()
    {
        // Общие параметры уже могут быть установлены в инспекторе или инициализироваться заранее
        CurrentHP = maxHP;
        currentDEF = baseDEF;
        currentEVA = baseEVA;
        currentSPD = baseSPD;

        currentATK = baseATK;
        currentACC = baseACC;
        currentCRIT = baseCRIT;

        CurrentSP = maxSP;
    }

    public void Init(EnemyStats enemyStats)
    {
        CurrentHP = maxHP = enemyStats.characterParameters.maxHP;
        currentDEF = enemyStats.characterParameters.DEF;
        currentEVA = enemyStats.characterParameters.EVA;
        currentSPD = enemyStats.characterParameters.SPD;

        currentATK = enemyStats.weaponParameters.ATK;
        currentACC = enemyStats.weaponParameters.ACC;
        currentCRIT = enemyStats.weaponParameters.CRIT;

        currentSE = maxSE = enemyStats.weaponParameters.SE;
        currentSEreg = enemyStats.weaponParameters.SEreg;
        currentSEdec = enemyStats.weaponParameters.SEdec;

        CurrentSP = enemyStats.characterParameters.SP;
        SPreg = enemyStats.characterParameters.SPreg;
        SPmovecost = enemyStats.characterParameters.SPmovecost;

        skillSet = enemyStats.skillSet;
    }

    // EnemyBattleCharacter.cs
    public SkillAsset GetSkill(int skillID)
    {
        foreach (var skill in skillSet.skills)
        {
            if (skill.GetInstanceID() == skillID)
                return skill;
        }
        return null;
    }

    public override int[] GetSkillIDs()
    {
        if (skillSet == null || skillSet.skills == null)
            return new int[0];

        return skillSet.skills
            .Where(skill => skill != null)
            .Select(skill => skill.GetInstanceID())
            .ToArray();
    }
}
