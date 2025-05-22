using UnityEngine;

public class SkillHandlerSP : MonoBehaviour
{
    public static SkillHandlerSP Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Выполняет выбранный навык в одиночном режиме.
    /// </summary>
    /// <param name="skill">Ассет навыка (SkillAsset)</param>
    /// <param name="user">Пользователь навыка</param>
    /// <param name="target">Целевая сущность</param>
    public void ExecuteSkill(SkillAsset skill, BattleEntitySP user, BattleEntitySP target)
    {
        ApplySkillLocally(skill, user, target);
    }

    private void ApplySkillLocally(SkillAsset skill, BattleEntitySP user, BattleEntitySP target)
    {
        if (skill.effect != null)
        {
            skill.effect.ApplyEffect(user, target);
            PlaySkillVisualEffects(skill, user, target);
        }
    }

    private void PlaySkillVisualEffects(SkillAsset skill, BattleEntitySP user, BattleEntitySP target)
    {
        // Локальное воспроизведение эффектов навыка
        Debug.Log($"Skill {skill.name} applied by {user.name} to {target?.name}");
    }
}