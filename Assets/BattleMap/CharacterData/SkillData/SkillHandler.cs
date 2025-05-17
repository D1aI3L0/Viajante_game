#if MIRROR
using Mirror;
#endif
using UnityEngine;

public class SkillHandler :
#if MIRROR
    NetworkBehaviour
#else
    MonoBehaviour
#endif
{
    public static SkillHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Выполняет выбранный навык.
    /// </summary>
    /// <param name="skill">Ассет навыка (SkillAsset)</param>
    /// <param name="user">Пользователь навыка (например, AllyBattleCharacter)</param>
    /// <param name="target">Целевая сущность (если применяется атакующий эффект)</param>
    public void ExecuteSkill(SkillAsset skill, BattleEntity user, BattleEntity target)
    {
#if MIRROR
        if (isLocalPlayer)
        {
            CmdExecuteSkill(skill.GetInstanceID(), user.netId, target?.netId);
        }
#else
        // Логика для одиночного режима
        ApplySkillLocally(skill, user, target);
#endif
    }
#if MIRROR
    [Command]
    private void CmdExecuteSkill(int skillId, uint userNetId, uint? targetNetId)
    {
        // Поиск объектов по NetworkIdentity
        NetworkServer.spawned.TryGetValue(userNetId, out NetworkIdentity userIdentity);
        NetworkIdentity targetIdentity = targetNetId.HasValue
            ? NetworkServer.spawned[targetNetId.Value]
            : null;

        if (userIdentity == null)
        {
            Debug.LogError("CmdExecuteSkill: userIdentity не найден");
            return;
        }

        BattleEntity userEntity = userIdentity.GetComponent<BattleEntity>();
        BattleEntity targetEntity = targetIdentity?.GetComponent<BattleEntity>();
        SkillAsset skill = SkillDatabase.Instance.GetSkillById(skillId);

        if (skill == null || userEntity == null)
        {
            Debug.LogError("CmdExecuteSkill: недопустимые параметры");
            return;
        }

        // Проверка валидности действия на сервере
        if (IsSkillValid(skill, userEntity, targetEntity))
        {
            RpcApplySkill(skillId, userNetId, targetNetId);
            ApplySkillLocally(skill, userEntity, targetEntity);
        }
    }

    [ClientRpc]
    private void RpcApplySkill(int skillId, uint userNetId, uint? targetNetId)
    {
        // Применение эффекта на всех клиентах
        BattleEntity user = NetworkClient.spawned[userNetId].GetComponent<BattleEntity>();
        BattleEntity target = targetNetId.HasValue
            ? NetworkClient.spawned[targetNetId.Value].GetComponent<BattleEntity>()
            : null;
        SkillAsset skill = SkillDatabase.Instance.GetSkillById(skillId);

        ApplySkillLocally(skill, user, target);
    }

    private bool IsSkillValid(SkillAsset skill, BattleEntity user, BattleEntity target)
    {
        // Проверка дистанции
        float distance = Vector3.Distance(user.currentCell.transform.position,
                                        target.currentCell.transform.position);
        if (distance > skill.maxRange) return false;

        // Проверка ресурсов
        if (user.CurrentSP < skill.baseSPCost) return false;

        // Дополнительные условия (например, Line of Sight)
        return true;
    }
#endif

    private void ApplySkillLocally(SkillAsset skill, BattleEntity user, BattleEntity target)
    {
        // Общая логика применения навыка
        if (skill.effect != null)
        {
            skill.effect.ApplyEffect(user, target);
            PlaySkillVisualEffects(skill, user, target);
        }
    }

    private void PlaySkillVisualEffects(SkillAsset skill, BattleEntity user, BattleEntity target)
    {
        // Синхронизация визуальных эффектов
#if MIRROR
        RpcPlaySkillEffects(skill.GetInstanceID(), user.netId, target?.netId);
#else
        // Локальное воспроизведение
        
#endif
    }

#if MIRROR
    [ClientRpc]
    private void RpcPlaySkillEffects(int skillId, uint userNetId, uint? targetNetId)
    {
        BattleEntity target = targetNetId.HasValue
            ? NetworkClient.spawned[targetNetId.Value].GetComponent<BattleEntity>()
            : null;
        SkillAsset skill = SkillDatabase.Instance.GetSkillById(skillId);
        Vector3 targetPos = target != null ? target.transform.position : Vector3.zero;
    }
#endif
}
