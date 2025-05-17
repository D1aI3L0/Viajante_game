#if MIRROR
using Mirror;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class TurnManager :
#if MIRROR
    NetworkBehaviour
#else
    MonoBehaviour
#endif
{
    public static TurnManager Instance { get; private set; }

    // Список участников боя.
#if MIRROR
    readonly public SyncList<uint> SyncParticipants = new SyncList<uint>();
#else
    public List<BattleEntity> participants = new List<BattleEntity>();
#endif

    public float turnThreshold = 100f;
    public bool battleActive = true;
    public float updateInterval = 0.1f;

#if MIRROR
    [SyncVar(hook = nameof(OnActiveUnitChanged))]
#endif
    public BattleEntity currentActiveUnit;
    private Coroutine gaugeCoroutine;

    public bool skipAutoInitialization = false;

    private void Awake()
    {
        if (skipAutoInitialization)
            return;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (skipAutoInitialization)
            return;
        StartGaugeCycle();
    }

    public void StatrTest()
    {
        Instance = this;
    }

    public void StartGaugeCycle()
    {

#if MIRROR
        if (isServer)
        {
            StartCoroutine(ServerUpdateTurnGauges());
        }
#else
        if (gaugeCoroutine != null)
        {
            StopCoroutine(gaugeCoroutine);
        }
        gaugeCoroutine = StartCoroutine(UpdateTurnGauges());
#endif
    }


#if MIRROR
    [Server]
    private IEnumerator ServerUpdateTurnGauges()
    {
        while (battleActive)
        {
            foreach (uint id in SyncParticipants)
            {
                BattleEntity entity = GetParticipantById(id); 
                if (entity == null) continue;

                // Только сервер увеличивает шкалу
                entity.turnGauge += entity.currentSPD * updateInterval;

                if (entity.turnGauge >= turnThreshold)
                {
                    currentActiveUnit = entity;
                    RpcSetActiveUnit(entity.netId);
                    yield break;
                }
            }
            yield return new WaitForSeconds(updateInterval);
        }
    }

    [ClientRpc]
    private void RpcSetActiveUnit(uint unitId)
    {
        NetworkServer.spawned.TryGetValue(unitId, out NetworkIdentity identity);
        currentActiveUnit = identity?.GetComponent<BattleEntity>();
    }
#else
    private IEnumerator UpdateTurnGauges()
    {
        while (battleActive)
        {
            foreach (BattleEntity entity in participants)
            {
                float effectiveSpeed = entity.currentSPD; // Можно добавить модификаторы скорости
                entity.turnGauge += effectiveSpeed * updateInterval;

                if (entity.turnGauge >= turnThreshold)
                {
                    currentActiveUnit = entity;
                    Debug.Log("TurnManager: Ход у " + entity.name);
                    if (BattleManager.Instance != null)
                    {
                        BattleManager.Instance.SetActiveUnit(entity);
                    }
                    yield break;
                }
            }
            yield return new WaitForSeconds(updateInterval);
        }
    }
#endif

    public void EndCurrentTurn()
    {
#if MIRROR
        if (isLocalPlayer)
        {
            CmdEndTurn();
        }
#else
        if (currentActiveUnit != null)
        {
            Debug.Log("TurnManager: Завершаем ход у " + currentActiveUnit.name);
            currentActiveUnit.turnGauge = 0;
            currentActiveUnit = null;
        }
        StartGaugeCycle();
#endif
    }

    public void RegisterParticipant(BattleEntity entity)
    {
#if MIRROR
        if (isServer)
        {
            NetworkServer.Spawn(entity.gameObject);
            SyncParticipants.Add(entity.netId);
        }
#else
        if (entity != null && !participants.Contains(entity))
        {
            participants.Add(entity);
            Debug.Log("TurnManager: Зарегистрирован участник: " + entity.name);
        }
#endif
    }

    public BattleEntity GetParticipantById(uint netId)
    {
        NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity identity);
        return identity?.GetComponent<BattleEntity>();
    }

    public void UnregisterParticipant(BattleEntity entity)
    {
#if MIRROR
        if (isServer)
        {
            SyncParticipants.Remove(entity.netId);
            NetworkServer.Destroy(entity.gameObject);
        }
#else
        if (participants.Contains(entity))
        {
            participants.Remove(entity);
            Debug.Log("TurnManager: Участник удалён: " + entity.name);
        }
#endif
    }

#if MIRROR
    private void OnActiveUnitChanged(BattleEntity oldUnit, BattleEntity newUnit)
    {

    }
#endif



#if MIRROR
    [Command(requiresAuthority = false)]
    public void CmdEndTurn()
    {
        if (currentActiveUnit != null)
        {
            // Сбрасываем шкалу только на сервере
            currentActiveUnit.turnGauge = 0;
            RpcResetTurnGauge(currentActiveUnit.netId);
            StartCoroutine(ServerUpdateTurnGauges());
        }
    }

    [ClientRpc]
    private void RpcResetTurnGauge(uint unitId)
    {
        NetworkServer.spawned.TryGetValue(unitId, out NetworkIdentity identity);
        if (identity != null)
        {
            identity.GetComponent<BattleEntity>().turnGauge = 0;
        }
    }
#endif

    public List<uint> GetParticipants()
    {
#if MIRROR
        return new List<uint>(SyncParticipants);
#else
        return participants;
#endif
    }
}