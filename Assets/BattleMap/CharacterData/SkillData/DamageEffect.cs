using UnityEngine;

[CreateAssetMenu(fileName = "DamageEffect", menuName = "Skills/Effects/Damage Effect")]
public class DamageEffect : SkillEffect
{
    [Tooltip("Минимальный множитель урона (например, 0.8 для 80%)")]
    public float damageMultiplierMin = 0.8f;
    
    [Tooltip("Максимальный множитель урона (например, 1.1 для 110%)")]
    public float damageMultiplierMax = 1.1f;
    
    [Tooltip("Бонус меткости навыка (например, 1.1 для 110%)")]
    public float skillAccuracyBonus = 1.1f;
    
    [Tooltip("Дополнительный бонус к критическому шансу (%), прибавляемый к базовому")]
    public float critBonus = 5f;

    public override void ApplyEffect(BattleEntity user, BattleEntity target)
    {
        if (user == null || target == null)
        {
            Debug.LogWarning("DamageEffect: отсутствует пользователь или цель");
            return;
        }
        
        int atk = 0;
        int acc = 0;
        int crit = 0;
        
        // Обязательно используем вычисляемые параметры из AllyBattleCharacter
        if (user is AllyBattleCharacter ally)
        {
            atk = ally.CurrentATK;
            acc = ally.CurrentACC;
            crit = ally.CurrentCRIT;
        }
        else
        {
            Debug.LogWarning("DamageEffect: пользователь не является AllyBattleCharacter, не удается получить актуальные параметры.");
            return;
        }
        
        float multiplier = Random.Range(damageMultiplierMin, damageMultiplierMax);
        float calculatedDamage = atk * multiplier;
        float effectiveAccuracy = acc * skillAccuracyBonus;
        float effectiveCrit = crit + critBonus;
        
        Debug.LogFormat("{0} применяет DamageEffect: базовая атака = {1}, множитель = {2:F2}, итоговый урон = {3:F1}. Меткость: {4:F1}%, крит: {5:F1}%", 
                         user.name, atk, multiplier, calculatedDamage, effectiveAccuracy, effectiveCrit);
        
        //target.TakeDamage(calculatedDamage);
    }
}
