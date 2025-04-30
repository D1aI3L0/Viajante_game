using System;


[Serializable]
public class WeaponUpgradeSkill : WeaponUpgrade 
{
    public bool isFixed;
    public Skill linkedSkill;

    public void Initialize(Skill skill)
    {
        linkedSkill = skill;
    }
}