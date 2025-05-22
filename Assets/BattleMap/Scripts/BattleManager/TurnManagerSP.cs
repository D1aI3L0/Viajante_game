using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TurnManagerSP : MonoBehaviour
{
    public static TurnManagerSP Instance { get; private set; }

    // Список участников боя (однопользовательская версия)
    public List<BattleEntitySP> participants = new List<BattleEntitySP>();

    // Порог, по достижении которого роль активного юнита переходит к текущему участнику
    public float turnThreshold = 100f;
    public bool battleActive = true;
    public float updateInterval = 0.1f;

    // Текущий активный юнит
    public BattleEntitySP currentActiveUnit;
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

    // Для тестовой инициализации
    public void StatrTest()
    {
        Instance = this;
    }

    /// <summary>
    /// Запускает цикл обновления шкалы хода
    /// </summary>
    public void StartGaugeCycle()
    {
        if (gaugeCoroutine != null)
        {
            StopCoroutine(gaugeCoroutine);
        }
        gaugeCoroutine = StartCoroutine(UpdateTurnGauges());
    }

    /// <summary>
    /// Однопользовательский цикл увеличения шкалы хода для каждого участника
    /// </summary>
    private IEnumerator UpdateTurnGauges()
    {
        while (battleActive)
        {
            foreach (BattleEntitySP entity in participants)
            {
                float effectiveSpeed = entity.currentSPD; // При необходимости можно добавить модификаторы
                entity.turnGauge += effectiveSpeed * updateInterval;

                if (entity.turnGauge >= turnThreshold)
                {
                    currentActiveUnit = entity;
                    Debug.Log("TurnManagerSP: Ход у " + entity.name);
                    // Передаём активного юнита в BattleManager (предполагается, что используется однопользовательская версия)
                    if (BattleManagerSP.Instance != null)
                    {
                        BattleManagerSP.Instance.SetActiveUnit(entity);
                    }
                    yield break;
                }
            }
            yield return new WaitForSeconds(updateInterval);
        }
    }

    /// <summary>
    /// Завершает ход текущего активного юнита и перезапускает цикл
    /// </summary>
    public void EndCurrentTurn()
    {
        if (currentActiveUnit != null)
        {
            Debug.Log("TurnManagerSP: Завершаем ход у " + currentActiveUnit.name);
            currentActiveUnit.turnGauge = 0;
            currentActiveUnit = null;
        }
        StartGaugeCycle();
    }

    /// <summary>
    /// Регистрирует участника боя (если его ещё нет в списке)
    /// </summary>
    public void RegisterParticipant(BattleEntitySP entity)
    {
        if (entity != null && !participants.Contains(entity))
        {
            participants.Add(entity);
            Debug.Log("TurnManagerSP: Зарегистрирован участник: " + entity.name);
        }
    }

    /// <summary>
    /// Удаляет участника из списка
    /// </summary>
    public void UnregisterParticipant(BattleEntitySP entity)
    {
        if (participants.Contains(entity))
        {
            participants.Remove(entity);
            Debug.Log("TurnManagerSP: Участник удалён: " + entity.name);
        }
    }

    /// <summary>
    /// Возвращает список всех участников
    /// </summary>
    public List<BattleEntitySP> GetParticipants()
    {
        return participants;
    }
}