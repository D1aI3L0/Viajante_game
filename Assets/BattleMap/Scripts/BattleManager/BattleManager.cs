#if MIRROR
using Mirror;
#endif
using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class BattleManager :
#if MIRROR
    NetworkBehaviour // Наследуемся от NetworkBehaviour в мультиплеере
#else
    MonoBehaviour    // В одиночном режиме остаётся MonoBehaviour
#endif
{
    public static BattleManager Instance { get; set; }

    // Списки участников для контроля глобального состояния боя
#if MIRROR
    readonly public SyncList<uint> SyncAllies = new SyncList<uint>();
    readonly public SyncList<uint> SyncEnemies = new SyncList<uint>();
    readonly public SyncList<uint> players = new SyncList<uint>();
#else
    public List<BattleEntity> Allies = new List<BattleEntity>();
    public List<BattleEntity> Enemies = new List<BattleEntity>();
#endif

    // Ссылка на TurnManager, который занимается переключением ходов
    public TurnManager turnManager;

    // Модули управления ходами: для игрока и для врагов (AI)
    public PlayerTurnController playerTurnController;
    public EnemyTurnController enemyTurnController;

    // Активный сейчас юнит, чей ход выполняется
    private BattleEntity activeUnit;

    public bool skipAutoInitialization = false;

    private void Awake()
    {
        if (skipAutoInitialization)
            return;
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
            turnManager = GetComponent<TurnManager>();
        }
        StartBattle();
    }

    public void StartTest()
    {
        Instance = this;
    }

    // Добавляем метод для получения всех сущностей
    public List<BattleEntity> GetAllEntities()
    {
#if MIRROR
        return null;
#else
        List<BattleEntity> entities = new List<BattleEntity>();

        // Добавляем союзников
        foreach (var ally in Allies.Where(a => a != null))
        {
            entities.Add(ally);
        }

        // Добавляем врагов
        foreach (var enemy in Enemies.Where(e => e != null))
        {
            entities.Add(enemy);
        }

        return entities;
#endif
    }

    /// <summary>
    /// Регистрирует союзного юнита в списке Allies.
    /// Вызывается при размещении союзников.
    /// </summary>
    public void RegisterAlly(BattleEntity ally)
    {
#if MIRROR
        if (isServer)
        {
            NetworkServer.Spawn(ally.gameObject);
            SyncAllies.Add(ally.netId); // Только сервер управляет списком
        }
#else
        if (ally != null && !Allies.Contains(ally))
        {
            Allies.Add(ally);
            Debug.Log("Зарегистрирован союзник: " + ally.name + ". Всего союзников: " + Allies.Count);
        }
#endif
    }

    /// <summary>
    /// Регистрирует вражеского юнита в списке Enemies.
    /// Вызывается при размещении врагов.
    /// </summary>
    public void RegisterEnemy(BattleEntity enemy)
    {
#if MIRROR
        if (isServer)
        {
            NetworkServer.Spawn(enemy.gameObject);
            SyncEnemies.Add(enemy.netId);
        }
#else
        if (enemy != null && !Enemies.Contains(enemy))
        {
            Enemies.Add(enemy);
            Debug.Log("Зарегистрирован враг: " + enemy.name + ". Всего врагов: " + Enemies.Count);
        }
#endif
    }

    public BattleEntity GetEntityByID(uint netId)
    {
        NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity identity);
        return identity?.GetComponent<BattleEntity>();
    }

    /// <summary>
    /// Вызывается TurnManager, когда выбран активный юнит.
    /// Здесь определяется, чей сейчас ход, и делегируется соответствующему модулю управления.
    /// </summary>
    public void SetActiveUnit(BattleEntity unit)
    {
        activeUnit = unit;
        Debug.Log("Активный юнит: " + unit.name);

        // Если юнит не враг (то есть, игрок), запускаем управление игроком
        if (!unit.IsEnemy)
        {
            if (playerTurnController != null)
            {
                playerTurnController.StartTurn(unit);
            }
        }
        // Если враг – запускаем AI управления для врагов
        else
        {
            if (enemyTurnController != null)
            {
                enemyTurnController.StartTurn((EnemyBattleCharacter)unit);
            }
        }
    }

    /// <summary>
    /// Вызывается по завершении хода активного юнита (из модуля управления).
    /// Сбрасывает активного юнита и сообщает TurnManager об окончании хода.
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
    /// Вызывается при старте боя.
    /// Здесь можно проводить первичную инициализацию и дополнительные проверки (например, проверить, что списки не пусты).
    /// </summary>
    public void StartBattle()
    {
#if MIRROR
        if (isServer)
        {
            BattleMapManager.Instance.ServerGenerateMap();
        }
#else
        BattleMapManager.Instance.CreateBattleMap();
        PlaceCharacters();
        turnManager.StartGaugeCycle();
#endif
    }

    /// <summary>
    /// Вызывается по завершении боя.
    /// Здесь можно проводить очистку данных, оповещать UI и т.п.
    /// </summary>
    public void EndBattle()
    {
        Debug.Log("Бой закончен");
    }

#if MIRROR
    [Server]
    public void ServerStartBattle()
    {
        // 1. Генерируем карту на сервере
        BattleMapManager.Instance.CreateBattleMap();

        // 2. Синхронизируем данные карты
        RpcSyncMapData(
            BattleMapManager.Instance.GetObstacleIndices(),
            BattleMapManager.Instance.GetSpawnZones()
        );

        // 3. Распределяем игроков по командам
        DistributePlayers();
    }

    [ClientRpc]
    private void RpcSyncMapData(int[] obstacleIndices, Vector2[] spawnZones)
    {
        // Клиенты воссоздают карту по данным сервера
        BattleMapManager.Instance.RecreateMap(obstacleIndices, spawnZones);
    }

    [Server]
    private void DistributePlayers()
    {
        foreach (NetworkConnection conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null) continue;

            NetworkPlayerController player = conn.identity.GetComponent<NetworkPlayerController>();
            if (player != null)
            {
                BattleCell spawnCell = BattleMapManager.Instance.GetFreeSpawnCell(isAlly: true);
                if (spawnCell != null)
                {
                    player.SpawnPlayerCharacter(spawnCell);
                }
            }
        }
    }
#endif

#if MIRROR
    [Server]
    public void ServerOnEntityDeath(BattleEntity entity)
    {
        // Удаляем сущность из списков
        if (SyncAllies.Contains(entity.netId)) SyncAllies.Remove(entity.netId);
        if (SyncEnemies.Contains(entity.netId)) SyncEnemies.Remove(entity.netId);

        // Оповещаем клиентов
        RpcEntityDeath(entity.netId);
    }

    [ClientRpc]
    private void RpcEntityDeath(uint entityId)
    {
        NetworkServer.spawned.TryGetValue(entityId, out NetworkIdentity identity);
        BattleEntity entity = identity?.GetComponent<BattleEntity>();
        if (entity != null)
        {

        }
    }
#endif

#if MIRROR
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

    public NetworkPlayerController GetPlayerByID(uint netId)
    {
        NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity identity);
        return identity?.GetComponent<NetworkPlayerController>();
    }

    [ClientRpc]
    private void RpcStartBattle()
    {
        // Клиенты получают оповещение о старте боя
    }
#endif

#if MIRROR
    public List<uint> GetAllAllies()
    {
        return new List<uint>(SyncAllies);
    }
#else
    public List<BattleEntity> GetAllAllies()
    {
        return Allies;
    }
#endif

#if MIRROR
    public SyncList<uint> GetAllEnemies()
    {
        return SyncEnemies;
    }
#endif
}