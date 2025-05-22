#if MIRROR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Mirror;

public class TurnManagerMP : NetworkBehaviour
{
    public static TurnManagerMP Instance { get; private set; }

    // Список участников боя (синхронизирован через сетевые ID)
    public readonly SyncList<uint> SyncParticipants = new SyncList<uint>();

    public float turnThreshold = 100f;
    public bool battleActive = true;
    public float updateInterval = 0.1f;

    // Синхронизируемый активный юнит (с помощью hook можно добавить логику при изменении)
    [SyncVar(hook = nameof(OnActiveUnitChanged))]
    public BattleEntityMP currentActiveUnit;
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

    /// <summary>
    /// Запускает серверный цикл обновления шкалы хода
    /// </summary>
    public void StartGaugeCycle()
    {
        if (isServer)
        {
            StartCoroutine(ServerUpdateTurnGauges());
        }
    }

    /// <summary>
    /// Цикл обновления шкалы хода, выполняемый на сервере
    /// </summary>
    [Server]
    private IEnumerator ServerUpdateTurnGauges()
    {
        while (battleActive)
        {
            foreach (uint id in SyncParticipants)
            {
                BattleEntityMP entity = GetParticipantById(id);
                if (entity == null)
                    continue;

                // Только сервер обновляет шкалу
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

    /// <summary>
    /// RPC‑метод для синхронизации активного юнита с клиентами
    /// </summary>
    [ClientRpc]
    private void RpcSetActiveUnit(uint unitId)
    {
        NetworkServer.spawned.TryGetValue(unitId, out NetworkIdentity identity);
        currentActiveUnit = identity?.GetComponent<BattleEntityMP>();
    }

    /// <summary>
    /// Метод для завершения хода. На локальном клиенте вызывается команда.
    /// </summary>
    public void EndCurrentTurn()
    {
        if (isLocalPlayer)
        {
            CmdEndTurn();
        }
    }

    /// <summary>
    /// Команда, вызываемая на сервере для завершения хода активного юнита
    /// </summary>
    [Command(requiresAuthority = false)]
    public void CmdEndTurn()
    {
        if (currentActiveUnit != null)
        {
            currentActiveUnit.turnGauge = 0;
            RpcResetTurnGauge(currentActiveUnit.netId);
            StartCoroutine(ServerUpdateTurnGauges());
        }
    }

    /// <summary>
    /// RPC‑метод для сброса шкалы хода активного юнита на клиентах
    /// </summary>
    [ClientRpc]
    private void RpcResetTurnGauge(uint unitId)
    {
        NetworkServer.spawned.TryGetValue(unitId, out NetworkIdentity identity);
        if (identity != null)
        {
            identity.GetComponent<BattleEntityMP>().turnGauge = 0;
        }
    }

    /// <summary>
    /// Регистрирует участника (только на сервере)
    /// </summary>
    public void RegisterParticipant(BattleEntityMP entity)
    {
        if (isServer)
        {
            NetworkServer.Spawn(entity.gameObject);
            SyncParticipants.Add(entity.netId);
        }
    }

    /// <summary>
    /// Возвращает участника по его сетевому ID
    /// </summary>
    public BattleEntityMP GetParticipantById(uint netId)
    {
        NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity identity);
        return identity?.GetComponent<BattleEntityMP>();
    }

    /// <summary>
    /// Удаляет участника – только сервер удаляет его из синхронизированного списка
    /// </summary>
    public void UnregisterParticipant(BattleEntityMP entity)
    {
        if (isServer)
        {
            SyncParticipants.Remove(entity.netId);
            NetworkServer.Destroy(entity.gameObject);
        }
    }

    // Hook для обработки изменений активного юнита (при необходимости можно добавить действия)
    private void OnActiveUnitChanged(BattleEntityMP oldUnit, BattleEntityMP newUnit)
    {
        // Здесь можно реализовать дополнительную логику при смене активного юнита
    }

    /// <summary>
    /// Возвращает список всех зарегистрированных участников (сетевые ID)
    /// </summary>
    public List<uint> GetParticipants()
    {
        return new List<uint>(SyncParticipants);
    }
}
#endif