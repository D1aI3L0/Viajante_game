using UnityEngine;

public class SkillEffect : ScriptableObject
{
    /// <summary>
    /// Выполняет эффект навыка.
    /// user – персонаж, применяющий навык; target – цель (если применимо)
    /// </summary>
    public virtual void ApplyEffect(BattleEntitySP user, BattleEntitySP target){}
    
    public virtual void ApplyEffect(BattleEntityMP user, BattleEntityMP target){}

    public virtual void Copy(SkillEffect other) { }
}
