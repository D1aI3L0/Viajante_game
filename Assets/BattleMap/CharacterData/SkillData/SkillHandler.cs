using UnityEngine;

public class SkillHandler : MonoBehaviour
{
    /// <summary>
    /// Выполняет выбранный навык.
    /// </summary>
    /// <param name="skill">Ассет навыка (SkillAsset)</param>
    /// <param name="user">Пользователь навыка (например, AllyBattleCharacter)</param>
    /// <param name="target">Целевая сущность (если применяется атакующий эффект)</param>
    public void ExecuteSkill(SkillAsset skill, BattleEntity user, BattleEntity target)
    {
        if (skill == null || user == null)
        {
            Debug.LogWarning("SkillHandler: отсутствует навык или пользователь");
            return;
        }
        
        // Если пользователь является AllyBattleCharacter, списываем базовые затраты SP
        if (user is AllyBattleCharacter ally)
        {
            if (ally.CurrentSP < skill.baseSPCost)
            {
                Debug.LogFormat("{0} недостаточно SP для применения навыка {1}.", ally.name, skill.skillName);
                return;
            }
            ally.CurrentSP -= skill.baseSPCost;
        }
        else
        {
            Debug.LogWarning("SkillHandler: пользователь не является AllyBattleCharacter — пропуск списания SP.");
        }
        
        Debug.LogFormat("{0} применяет навык {1}.", user.name, skill.skillName);
        foreach (SkillEffect effect in skill.effects)
        {
            if (effect != null)
            {
                // Если у эффекта есть дополнительная стоимость, списываем её
                if (user is AllyBattleCharacter a)
                {
                    if (a.CurrentSP >= effect.additionalSPCost)
                    {
                        a.CurrentSP -= effect.additionalSPCost;
                    }
                    else
                    {
                        Debug.LogFormat("{0} недостаточно SP для выполнения эффекта {1}.", a.name, effect.name);
                        continue;
                    }
                }
                effect.ApplyEffect(user, target);
            }
        }
    }
}
