using UnityEngine;

[CreateAssetMenu(fileName = "MovementEffect", menuName = "Skills/Effects/Movement Effect")]
public class MovementEffect : SkillEffect
{
    [Tooltip("Расстояние перемещения")]
    public float moveDistance = 5f;

    public override void ApplyEffect(BattleEntitySP user, BattleEntitySP target)
    {
        if (user == null)
        {
            Debug.LogWarning("MovementEffect: отсутствует пользователь");
            return;
        }
        
        Vector3 originalPosition = user.transform.position;
        user.transform.position += user.transform.forward * moveDistance;
        Debug.LogFormat("{0} переместился с {1} на {2}.", user.name, originalPosition, user.transform.position);
    }

    public override void ApplyEffect(BattleEntityMP user, BattleEntityMP target)
    {
        if (user == null)
        {
            Debug.LogWarning("MovementEffect: отсутствует пользователь");
            return;
        }
        
        Vector3 originalPosition = user.transform.position;
        user.transform.position += user.transform.forward * moveDistance;
        Debug.LogFormat("{0} переместился с {1} на {2}.", user.name, originalPosition, user.transform.position);
    }

    public override void Copy(SkillEffect other)
    {
        moveDistance = ((MovementEffect)other).moveDistance;
    }
}
