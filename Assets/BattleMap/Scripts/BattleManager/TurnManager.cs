using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    // Список участников боя.
    public List<BattleEntity> participants = new List<BattleEntity>();

    public float turnThreshold = 100f;
    public bool battleActive = true;
    public float updateInterval = 0.1f;

    public BattleEntity currentActiveUnit;
    private Coroutine gaugeCoroutine;

    private void Awake()
    {
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

    public void StartGaugeCycle()
    {
        if (gaugeCoroutine != null)
        {
            StopCoroutine(gaugeCoroutine);
        }
        gaugeCoroutine = StartCoroutine(UpdateTurnGauges());
    }

    IEnumerator UpdateTurnGauges()
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

    public void EndCurrentTurn()
    {
        if (currentActiveUnit != null)
        {
            Debug.Log("TurnManager: Завершаем ход у " + currentActiveUnit.name);
            currentActiveUnit.turnGauge = 0;
            currentActiveUnit = null;
        }
        StartGaugeCycle();
    }

    public void RegisterParticipant(BattleEntity entity)
    {
        if (entity != null && !participants.Contains(entity))
        {
            participants.Add(entity);
            Debug.Log("TurnManager: Зарегистрирован участник: " + entity.name);
        }
    }

    public void UnregisterParticipant(BattleEntity entity)
    {
        if (participants.Contains(entity))
        {
            participants.Remove(entity);
            Debug.Log("TurnManager: Участник удалён: " + entity.name);
        }
    }
}
