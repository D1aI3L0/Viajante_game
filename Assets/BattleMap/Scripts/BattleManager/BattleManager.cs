using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public List<BattleEntity> Allies = new List<BattleEntity>();
    public List<BattleEntity> Enemies = new List<BattleEntity>();

    public TurnManager turnManager;

    private void Awake()
    {
        // Устанавливаем singleton, если его ещё нет
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Если BattleManager находится только на боевой сцене, DontDestroyOnLoad можно не использовать
    }

    private void Start()
    {
        if (turnManager == null)
        {
            turnManager = GetComponent<TurnManager>();
        }
        StartBattle();
    }

    public void RegisterAlly(BattleEntity ally)
    {
        if (ally != null && !Allies.Contains(ally))
        {
            Allies.Add(ally);
            Debug.Log("Зарегистрирован союзник: " + ally.name + ". Всего союзников: " + Allies.Count);
        }
    }

    public void RegisterEnemy(BattleEntity enemy)
    {
        if (enemy != null && !Enemies.Contains(enemy))
        {
            Enemies.Add(enemy);
            Debug.Log("Зарегистрирован враг: " + enemy.name + ". Всего врагов: " + Enemies.Count);
        }
    }

    public void StartBattle()
    {
        Debug.Log("Бой начался");
        if (turnManager != null)
        {
            turnManager.BuildTurnOrder();
            turnManager.SetActiveTurn(0);
        }
    }

    public void EndBattle()
    {
        Debug.Log("Бой закончен");
    }
}
