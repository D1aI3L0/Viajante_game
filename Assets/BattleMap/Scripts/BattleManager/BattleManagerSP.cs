using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class BattleManagerSP : MonoBehaviour
{
    public static BattleManagerSP Instance { get; private set; }

    // Списки участников (союзников и врагов) для одиночного режима
    public List<BattleEntitySP> Allies = new List<BattleEntitySP>();
    public List<BattleEntitySP> Enemies = new List<BattleEntitySP>();

    // Компоненты для управления ходами
    public TurnManagerSP turnManager;
    public PlayerTurnController playerTurnController;
    public EnemyTurnController enemyTurnController;

    // Активный в данный момент юнит
    private BattleEntitySP activeUnit;

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
            turnManager = FindFirstObjectByType<TurnManagerSP>();
        }
        if (playerTurnController == null)
        {
            playerTurnController = FindFirstObjectByType<PlayerTurnController>();
        }
        if (enemyTurnController == null)
        {
            enemyTurnController = FindFirstObjectByType<EnemyTurnController>();
        }
        StartBattle();
    }

    public void StartTest()
    {
        Instance = this;
    }

    /// <summary>
    /// Возвращает список всех участников (союзников и врагов).
    /// </summary>
    public List<BattleEntitySP> GetAllEntities()
    {
        // Объединяем списки союзников и врагов, исключая null-ссылки.
        List<BattleEntitySP> entities = new List<BattleEntitySP>(Allies.Where(a => a != null));
        entities.AddRange(Enemies.Where(e => e != null));
        return entities;
    }

    /// <summary>
    /// Регистрирует союзного юнита в списке Allies.
    /// Вызывается при размещении персонажей.
    /// </summary>
    public void RegisterAlly(BattleEntitySP ally)
    {
        if (ally != null && !Allies.Contains(ally))
        {
            Allies.Add(ally);
            Debug.Log("Зарегистрирован союзник: " + ally.name + ". Всего союзников: " + Allies.Count);
        }
    }

    /// <summary>
    /// Регистрирует вражеского юнита в списке Enemies.
    /// Вызывается при размещении врагов.
    /// </summary>
    public void RegisterEnemy(BattleEntitySP enemy)
    {
        if (enemy != null && !Enemies.Contains(enemy))
        {
            Enemies.Add(enemy);
            Debug.Log("Зарегистрирован враг: " + enemy.name + ". Всего врагов: " + Enemies.Count);
        }
    }

    /// <summary>
    /// Устанавливает активного юнита и передаёт управление соответствующему модулю.
    /// </summary>
    public void SetActiveUnit(BattleEntitySP unit)
    {
        activeUnit = unit;
        unit.isActiveTurn = true;
        Debug.Log("Активный юнит: " + unit.name);

        // Если юнит – игрок, запускается управление игроком
        if (!unit.IsEnemy)
        {
            if (playerTurnController != null)
            {
                playerTurnController.StartTurn(unit);
            }
        }
        // Если юнит – враг, запускается AI для врагов
        else
        {
            if (enemyTurnController != null)
            {
                enemyTurnController.StartTurn((EnemyBattleCharacterSP)unit);
            }
        }
    }

    /// <summary>
    /// Вызывается по завершении хода активного юнита.
    /// Обновляет статистику юнита и сообщает TurnManager.
    /// </summary>
    public void OnTurnComplete()
    {
        if (activeUnit != null)
        {
            UnitStatManager.Instance.ProcessEndOfTurn(activeUnit);
        }
        activeUnit.isActiveTurn = false;
        activeUnit = null;
        if (turnManager != null)
        {
            turnManager.EndCurrentTurn();
        }
    }

    /// <summary>
    /// Запускает битву: создаёт карту и запускает цикл переключения ходов.
    /// </summary>
    public void StartBattle()
    {
        // Предполагается, что для одиночного режима используется BattleMapManagerSP
        if (BattleMapManagerSP.Instance != null)
        {
            BattleMapManagerSP.Instance.CreateBattleMap();
        }
        if (turnManager != null)
        {
            turnManager.StartGaugeCycle();
        }
    }

    /// <summary>
    /// Вызывается по завершении битвы.
    /// Здесь можно реализовать очистку данных и уведомление UI.
    /// </summary>
    public void EndBattle()
    {
        Debug.Log("Бой закончен");
    }
}