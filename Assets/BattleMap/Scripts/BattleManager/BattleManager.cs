using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // Списки участников для контроля глобального состояния боя
    public List<BattleEntity> Allies = new List<BattleEntity>();
    public List<BattleEntity> Enemies = new List<BattleEntity>();

    // Ссылка на TurnManager, который занимается переключением ходов
    public TurnManager turnManager;

    // Модули управления ходами: для игрока и для врагов (AI)
    public PlayerTurnController playerTurnController;
    public EnemyTurnController enemyTurnController;

    // Активный сейчас юнит, чей ход выполняется
    private BattleEntity activeUnit;

    private void Awake()
    {
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
        if (turnManager == null)
        {
            turnManager = GetComponent<TurnManager>();
        }
        StartBattle();
    }

    /// <summary>
    /// Регистрирует союзного юнита в списке Allies.
    /// Вызывается при размещении союзников.
    /// </summary>
    public void RegisterAlly(BattleEntity ally)
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
    public void RegisterEnemy(BattleEntity enemy)
    {
        if (enemy != null && !Enemies.Contains(enemy))
        {
            Enemies.Add(enemy);
            Debug.Log("Зарегистрирован враг: " + enemy.name + ". Всего врагов: " + Enemies.Count);
        }
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
            else
            {
                Debug.LogWarning("PlayerTurnController не назначен!");
            }
        }
        // Если враг – запускаем AI управления для врагов
        else
        {
            if (enemyTurnController != null)
            {
                enemyTurnController.StartTurn(unit);
            }
            else
            {
                Debug.LogWarning("EnemyTurnController не назначен!");
                // Если контроллер отсутствует, можно сразу завершить ход
                OnTurnComplete();
            }
        }
    }

    /// <summary>
    /// Вызывается по завершении хода активного юнита (из модуля управления).
    /// Сбрасывает активного юнита и сообщает TurnManager об окончании хода.
    /// </summary>
    public void OnTurnComplete()
    {
        Debug.Log("Ход завершён.");
        activeUnit = null;

        // Здесь можно добавить проверки на окончание боя (например, если Allies.Count == 0 или Enemies.Count == 0)
        // Далее сообщаем TurnManager о завершении хода для перехода к следующему.
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
        Debug.Log("Бой начался");
        // Можно проверить, что списки участников корректно заполнены.
        // Далее запускаем TurnManager (например, старт накопления gauge).
        if (turnManager != null)
        {
            // turnManager.StartGaugeCycle(); // Если метод называется иначе, использовать его.
        }
    }

    /// <summary>
    /// Вызывается по завершении боя.
    /// Здесь можно проводить очистку данных, оповещать UI и т.п.
    /// </summary>
    public void EndBattle()
    {
        Debug.Log("Бой закончен");
    }
}
