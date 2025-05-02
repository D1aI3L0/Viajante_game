using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    // Список всех участников (BattleEntity – базовый класс для союзных и вражеских юнитов)
    public List<BattleEntity> participants = new List<BattleEntity>();

    public float turnThreshold = 100f;    // Порог для активации хода
    public bool battleActive = true;        // Флаг активности боя
    public float updateInterval = 0.1f;       // Интервал обновления gauge (в секундах)

    public BattleEntity currentActiveUnit;  // Текущий юнит, чей ход активируется

    private Coroutine gaugeCoroutine;

    private void Awake()
    {
        // Реализация синглтона для TurnManager
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        StartGaugeCycle();
    }

    /// <summary>
    /// Запускает корутину для обновления gauge.
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
    /// Корутина обновляет gauge у всех участников и останавливается, когда один из них достиг порога.
    /// </summary>
    IEnumerator UpdateTurnGauges()
    {
        while (battleActive)
        {
            foreach (BattleEntity entity in participants)
            {
                // Вычисляем эффективную скорость (при необходимости здесь можно добавить модификаторы)
                float effectiveSpeed = entity.currentSPD;  
                entity.turnGauge += effectiveSpeed * updateInterval;

                if (entity.turnGauge >= turnThreshold)
                {
                    currentActiveUnit = entity;
                    Debug.Log("Ход у: " + entity.name);

                    // Уведомляем BattleManager о текущем активном юните
                    if (BattleManager.Instance != null)
                    {
                        BattleManager.Instance.SetActiveUnit(entity);
                    }
                    
                    // Прерываем корутину, чтобы остановить дальнейшее обновление gauge пока юнит не завершит ход.
                    yield break;
                }
            }
            yield return new WaitForSeconds(updateInterval);
        }
    }

    /// <summary>
    /// Вызывается после того, как активный юнит завершил свой ход.
    /// Сбрасывает gauge и перезапускает обновление.
    /// </summary>
    public void EndCurrentTurn()
    {
        if (currentActiveUnit != null)
        {
            Debug.Log("Ход завершён у: " + currentActiveUnit.name);
            currentActiveUnit.turnGauge = 0;
            currentActiveUnit = null;
        }
        StartGaugeCycle();
    }

    /// <summary>
    /// Регистрирует участника в списке TurnManager.
    /// </summary>
    public void RegisterParticipant(BattleEntity entity)
    {
        if (entity != null && !participants.Contains(entity))
        {
            participants.Add(entity);
            Debug.Log("TurnManager: Зарегистрирован участник " + entity.name);
        }
    }

    /// <summary>
    /// Удаляет участника из списка TurnManager.
    /// </summary>
    public void UnregisterParticipant(BattleEntity entity)
    {
        if (participants.Contains(entity))
        {
            participants.Remove(entity);
            Debug.Log("TurnManager: Участник " + entity.name + " удалён");
        }
    }
}
