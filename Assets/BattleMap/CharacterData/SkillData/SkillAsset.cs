#if MIRROR
using Mirror;
#endif
using UnityEngine;

public enum SkillType
{
    Attack,
    Movement,
    Buff,
    Debuff
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "Skills/Skill Asset")]
public class SkillAsset : ScriptableObject
{
    [Header("Основная информация")]
    public string skillName;

    [Tooltip("Иконка навыка для отображения в UI")]
    public Sprite skillIcon;

    [Tooltip("Описание навыка, которое будет отображаться игроку")]
    [TextArea]
    public string description;

    [Tooltip("Базовая стоимость обычной энергии (SP)")]
    public int baseSPCost;

    [Tooltip("Базовая стоимость особой энергии (SE)")]
    public int baseSECost;

    public int maxRange;

    [Tooltip("Cooldown навыка (если используется)")]
    public int cooldown;
#if MIRROR
    [SyncVar]
#endif
    public int currentCooldown; // показатель текущего восстановления навыка

    [Tooltip("Тип навыка")]
    public SkillType skillType;

    [Header("Эффект навыка")]
    public SkillEffect effect;

    public SkillAsset(SkillAsset other)
    {
        skillName = other.skillName;
        skillIcon = other.skillIcon;
        description = other.description;
        baseSPCost = other.baseSECost;
        cooldown = other.cooldown;
        skillType = other.skillType;

        if (other.effect is DamageEffect)
        {
            effect = CreateInstance<DamageEffect>();
            effect.Copy(other.effect);
        }
        else if (other.effect is MovementEffect)
        {
            effect = CreateInstance<MovementEffect>();
            effect.Copy(other.effect);
        }
    }

    public void ResetCooldown()
    {
        currentCooldown = cooldown;
    }
}
