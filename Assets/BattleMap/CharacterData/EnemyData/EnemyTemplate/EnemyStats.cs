using System;
using UnityEngine;


[Serializable]
public class EnemyStats
{
    public CharacterParameters characterParameters = new();
    public WeaponParameters weaponParameters = new();

    public WeaponSkillSet skillSet;
}
