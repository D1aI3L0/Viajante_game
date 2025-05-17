using UnityEngine;

[System.Serializable]
public class WeaponSkillSelection
{
    [Tooltip("Массив из 3 индексов выбранных дополнительных навыков для данного оружия. Базовая атака всегда имеет индекс 0.")]
    public int[] selectedSkillIndices = new int[3] { 1, 2, 3 };
}
