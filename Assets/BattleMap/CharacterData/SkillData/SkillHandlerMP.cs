using Mirror;
using UnityEngine;

public class SkillHandlerMP : NetworkBehaviour
{
    public static SkillHandlerMP Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Выполняет выбранный навык в многопользовательском режиме.
    /// </summary>
    /// <param name="skill">Ассет навыка (SkillAsset)</param>
    /// <param name="user">Пользователь навыка</param>
    /// <param name="target">Целевая сущность</param>
    public void ExecuteSkill(SkillAsset skill, BattleEntityMP user, BattleEntityMP target)
    {
        if (isLocalPlayer)
        {
            CmdExecuteSkill(skill.GetInstanceID(), user.netId, target?.netId);
        }
    }

    [Command]
    private void CmdExecuteSkill(int skillId, uint userNetId, uint? targetNetId)
    {
        NetworkServer.spawned.TryGetValue(userNetId, out NetworkIdentity userIdentity);
        NetworkIdentity targetIdentity = targetNetId.HasValue
            ? NetworkServer.spawned[targetNetId.Value]
            : null;

        if (userIdentity == null)
        {
            Debug.LogError("CmdExecuteSkill: userIdentity не найден");
            return;
        }

        BattleEntityMP userEntity = userIdentity.GetComponent<BattleEntityMP>();
        BattleEntityMP targetEntity = targetIdentity?.GetComponent<BattleEntityMP>();
        SkillAsset skill = SkillDatabase.Instance.GetSkillById(skillId);

        if (skill == null || userEntity == null)
        {
            Debug.LogError("CmdExecuteSkill: недопустимые параметры");
            return;
        }

        if (IsSkillValid(skill, userEntity, targetEntity))
        {
            RpcApplySkill(skillId, userNetId, targetNetId);
            ApplySkillLocally(skill, userEntity, targetEntity);
        }
    }

    [ClientRpc]
    private void RpcApplySkill(int skillId, uint userNetId, uint? targetNetId)
    {
        BattleEntityMP user = NetworkClient.spawned[userNetId].GetComponent<BattleEntityMP>();
        BattleEntityMP target = targetNetId.HasValue
            ? NetworkClient.spawned[targetNetId.Value].GetComponent<BattleEntityMP>()
            : null;
        SkillAsset skill = SkillDatabase.Instance.GetSkillById(skillId);

        ApplySkillLocally(skill, user, target);
    }

    private bool IsSkillValid(SkillAsset skill, BattleEntityMP user, BattleEntityMP target)
    {
        float distance = Vector3.Distance(user.currentCell.transform.position, target.currentCell.transform.position);
        if (distance > skill.maxRange) return false;

        if (user.CurrentSP < skill.baseSPCost) return false;

        return true;
    }

    private void ApplySkillLocally(SkillAsset skill, BattleEntityMP user, BattleEntityMP target)
    {
        if (skill.effect != null)
        {
            skill.effect.ApplyEffect(user, target);
            PlaySkillVisualEffects(skill, user, target);
        }
    }

    private void PlaySkillVisualEffects(SkillAsset skill, BattleEntityMP user, BattleEntityMP target)
    {
        RpcPlaySkillEffects(skill.GetInstanceID(), user.netId, target?.netId);
    }

    [ClientRpc]
    private void RpcPlaySkillEffects(int skillId, uint userNetId, uint? targetNetId)
    {
        BattleEntityMP target = targetNetId.HasValue
            ? NetworkClient.spawned[targetNetId.Value].GetComponent<BattleEntityMP>()
            : null;
        SkillAsset skill = SkillDatabase.Instance.GetSkillById(skillId);
        Debug.Log($"Skill {skill.name} applied by {NetworkClient.spawned[userNetId].name} to {target?.name}");
    }
}