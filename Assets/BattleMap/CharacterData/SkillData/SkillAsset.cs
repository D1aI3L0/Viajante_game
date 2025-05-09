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
    
    [Tooltip("Cooldown навыка (если используется)")]
    public int cooldown;
    public int currentCooldown; // показатель текущего восстановления навыка
    
    [Tooltip("Тип навыка")]
    public SkillType skillType;

    [Header("Модули (эффекты) навыка")]
    [Tooltip("Массив модулей-эффектов, которые выполняются при активации навыка")]
    public SkillEffect[] effects;
}
