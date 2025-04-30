using System;
using UnityEngine;

[Serializable]
public class WeaponSkillSet
{
    [Tooltip("Набор навыков для конкретного оружия. Здесь, например, можно задать 5 навыков.")]
    public SkillParameters[] skills = new SkillParameters[5];
}
