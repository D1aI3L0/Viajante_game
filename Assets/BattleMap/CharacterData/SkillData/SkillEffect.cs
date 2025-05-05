using UnityEngine;

public abstract class SkillEffect : ScriptableObject
{
    [Tooltip("Дополнительная затрата SP для этого эффекта")]
    public int additionalSPCost;

    /// <summary>
    /// Выполняет эффект навыка.
    /// user – персонаж, применяющий навык; target – цель (если применимо)
    /// </summary>
    public abstract void ApplyEffect(BattleEntity user, BattleEntity target);
}
