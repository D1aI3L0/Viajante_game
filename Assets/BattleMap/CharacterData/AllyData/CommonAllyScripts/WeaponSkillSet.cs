using UnityEngine;
using System;

[Serializable]
public class WeaponSkillSet
{
    [Tooltip("Массив ассетов навыков для конкретного оружия. Здесь, например, можно задать 5 навыков.")]
    public SkillAsset[] skills = new SkillAsset[5];
}
