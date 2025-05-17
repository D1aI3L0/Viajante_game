using UnityEngine;

public class SkillEffect : ScriptableObject
{
    /// <summary>
    /// Выполняет эффект навыка.
    /// user – персонаж, применяющий навык; target – цель (если применимо)
    /// </summary>
    public virtual void ApplyEffect(BattleEntity user, BattleEntity target){}

    public virtual void Copy(SkillEffect other){}
}
