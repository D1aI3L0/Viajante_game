using UnityEngine;
using System;

[Serializable]
public class WeaponSkillSet
{
    [Tooltip("Массив ассетов навыков для конкретного оружия. Здесь, например, можно задать 5 навыков.")]
    public SkillAsset[] skills = new SkillAsset[5];

    public WeaponSkillSet(){}

    public WeaponSkillSet(WeaponSkillSet other)
    {
        for(int i = 0; i < other.skills.Length; i++)
        {
            skills[i] = other.skills[i];
        }
    }
}
