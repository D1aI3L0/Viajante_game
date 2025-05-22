#if MIRROR
using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class BattleManagerMP : NetworkBehaviour
{
    public static BattleManagerMP Instance { get; private set; }

    // Списки участников в виде синхронизируемых коллекций для мультиплеера
    public readonly SyncList<uint> SyncAllies = new SyncList<uint>();
    public readonly SyncList<uint> SyncEnemies = new SyncList<uint>();
    public readonly SyncList<uint> players = new SyncList<uint>();

    public GameObject networkPlayerPrefab;

    // Компоненты для управления ходами
    public TurnManagerMP turnManager;
    public PlayerTurnController playerTurnController;
    public EnemyTurnController enemyTurnController;

    private BattleEntityMP activeUnit;

    public bool skipAutoInitialization = false;

    private void Awake()
    {
        if (skipAutoInitialization)
            return;

        if (BattleConfig.IsMultiplayer)
        {
            // Создаём объект NetworkManager, если его ещё нет, и назначаем префаб игрока
            GameObject networkManagerObject = new GameObject("NetworkManager");
            NetworkManager manager = networkManagerObject.AddComponent<NetworkManager>();
            gameObject.AddComponent<NetworkIdentity>();
            manager.playerPrefab = networkPlayerPrefab;
        }

        // Реализация синглтона
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
        if (turnManager == null)
        {
            turnManager = GetComponent<TurnManagerMP>();
        }
        if (playerTurnController == null)
        {
            playerTurnController = GetComponent<PlayerTurnController>();
        }
        if (enemyTurnController == null)
        {
            enemyTurnController = GetComponent<EnemyTurnController>();
        }
        StartBattle();
    }

    public void StartTest()
    {
        Instance = this;
    }

    /// <summary>
    /// Для мультиплеер-версии метод возвращает null – список участников строится через SyncLists.
    /// </summary>
    public List<BattleEntityMP> GetAllEntities()
    {
        return null;
    }

    /// <summary>
    /// Регистрирует союзного юнита и отправляет его на сервер.
    /// </summary>
    public void RegisterAlly(BattleEntityMP ally)
    {
        if (isServer)
        {
            NetworkServer.Spawn(ally.gameObject);
            SyncAllies.Add(ally.netId);
        }
    }

    /// <summary>
    /// Регистрирует вражеского юнита и отправляет его на сервер.
    /// </summary>
    public void RegisterEnemy(BattleEntityMP enemy)
    {
        if (isServer)
        {
            NetworkServer.Spawn(enemy.gameObject);
            SyncEnemies.Add(enemy.netId);
        }
    }

    /// <summary>
    /// Получает BattleEntity по сетевому ID.
    /// </summary>
    public BattleEntityMP GetEntityByID(uint netId)
    {
        NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity identity);
        return identity?.GetComponent<BattleEntityMP>();
    }

    /// <summary>
    /// Устанавливает активного юнита и делегирует управление подходящему контроллеру.
    /// </summary>
    public void SetActiveUnit(BattleEntityMP unit)
    {
        activeUnit = unit;
        Debug.Log("Активный юнит: " + unit.name);

    }

    /// <summary>
    /// Завершает ход текущего активного юнита.
    /// </summary>
    public void OnTurnComplete()
    {
        if (activeUnit != null)
        {
            UnitStatManager.Instance.ProcessEndOfTurn(activeUnit);
        }
        activeUnit = null;
        if (turnManager != null)
        {
            turnManager.EndCurrentTurn();
        }
    }

    /// <summary>
    /// Запускает битву (вызывается сервером), генерируя карту и синхронизируя данные с клиентами.
    /// </summary>
    public void StartBattle()
    {
        if (isServer)
        {
            // Предполагается, что для мультиплеера используется BattleMapManagerMP.
            BattleMapManagerMP.Instance.ServerGenerateMap();
        }
    }

    /// <summary>
    /// Завершает битву.
    /// </summary>
    public void EndBattle()
    {
        Debug.Log("Бой закончен");
    }

    //////////////////////////////////////////////////////////////////////////////////
    // Методы для серверной и клиентской логики (с использованием Mirror)
    //////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Метод, вызываемый на сервере для старта битвы с генерацией карты и распределением игроков.
    /// </summary>
    [Server]
    public void ServerStartBattle()
    {
        // 1. Генерируем карту на сервере
        BattleMapManagerMP.Instance.ServerGenerateMap();

        // 2. Синхронизируем данные карты
        RpcSyncMapData(
            BattleMapManagerMP.Instance.GetObstacleIndices(),
            BattleMapManagerMP.Instance.GetSpawnZones()
        );

        // 3. Распределяем игроков по командам
        DistributePlayers();
    }

    /// <summary>
    /// RPC-метод для синхронизации данных карты с клиентами.
    /// </summary>
    [ClientRpc]
    private void RpcSyncMapData(int[] obstacleIndices, Vector2[] spawnZones)
    {
        BattleMapManagerMP.Instance.RecreateMap(obstacleIndices, spawnZones);
    }

    /// <summary>
    /// Распределяет игроков по командам, выбирая для каждого свободную ячейку спавна.
    /// </summary>
    [Server]
    private void DistributePlayers()
    {
        foreach (NetworkConnection conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null)
                continue;

            NetworkPlayerController player = conn.identity.GetComponent<NetworkPlayerController>();
            if (player != null)
            {
                BattleCell spawnCell = BattleMapManagerMP.Instance.GetFreeSpawnCell(isAlly: true);
                if (spawnCell != null)
                {
                    player.SpawnPlayerCharacter(spawnCell);
                }
            }
        }
    }

    /// <summary>
    /// Вызывается на сервере при смерти сущности – удаляет ID из синхронизируемых списков и оповещает клиентов.
    /// </summary>
    [Server]
    public void ServerOnEntityDeath(BattleEntityMP entity)
    {
        if (SyncAllies.Contains(entity.netId))
            SyncAllies.Remove(entity.netId);
        if (SyncEnemies.Contains(entity.netId))
            SyncEnemies.Remove(entity.netId);

        RpcEntityDeath(entity.netId);
    }

    /// <summary>
    /// RPC-метод, оповещающий клиентов о смерти сущности.
    /// </summary>
    [ClientRpc]
    private void RpcEntityDeath(uint entityId)
    {
        NetworkServer.spawned.TryGetValue(entityId, out NetworkIdentity identity);
        BattleEntityMP entity = identity?.GetComponent<BattleEntityMP>();
        if (entity != null)
        {
            // При необходимости добавить логику обработки смерти сущности
        }
    }

    /// <summary>
    /// Проверяет, готовы ли все игроки, и при успехе запускает старт битвы.
    /// </summary>
    [Server]
    public bool ServerCheckPlayersReady()
    {
        bool allReady = true;
        foreach (uint playerId in players)
        {
            NetworkPlayerController player = GetPlayerByID(playerId);
            if (player && !player.isReady)
            {
                allReady = false;
                break;
            }
        }

        if (allReady)
        {
            RpcStartBattle();
            if (!skipAutoInitialization)
                ServerStartBattle();
        }
        return allReady;
    }

    /// <summary>
    /// Получает NetworkPlayerController по сетевому ID.
    /// </summary>
    public NetworkPlayerController GetPlayerByID(uint netId)
    {
        NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity identity);
        return identity?.GetComponent<NetworkPlayerController>();
    }

    /// <summary>
    /// RPC-метод для оповещения клиентов о старте битвы.
    /// </summary>
    [ClientRpc]
    private void RpcStartBattle()
    {
        // Клиенты получают сообщение о старте битвы.
    }

    /// <summary>
    /// Возвращает список всех союзных юнитов в виде копии SyncList.
    /// </summary>
    public List<uint> GetAllAllies()
    {
        return new List<uint>(SyncAllies);
    }

    /// <summary>
    /// Возвращает синхронизируемый список вражеских юнитов.
    /// </summary>
    public SyncList<uint> GetAllEnemies()
    {
        return SyncEnemies;
    }
}
#endif