using UnityEngine;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // Списки участников, если вы хотите регистрировать их здесь
    public List<BattleEntity> allEntities = new List<BattleEntity>();

    // Ссылка на TurnManager (можно либо получить компонент на том же объекте, либо назначить в инспекторе)
    public TurnManager turnManager;

    private void Awake()
    {
        // Простой Singleton (если требуется)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        // Например, инициализация или оповещение о начале боя
        // Можно вызвать turnManager.BuildTurnOrder() или подобное при запуске боя.
        if (turnManager == null)
        {
            turnManager = GetComponent<TurnManager>();
        }
        // Начинаем бой,
        StartBattle();
    }
    
    public void StartBattle()
    {
        Debug.Log("Бой начался");
        // Можно тут проводить первичную регистрацию юнитов,
        // организовать начальную очередность и т.п.
        if (turnManager != null)
        {
            turnManager.BuildTurnOrder();
            turnManager.SetActiveTurn(0); // или другой подходящий метод для установки начального хода
        }
    }

    public void EndBattle()
    {
        Debug.Log("Бой закончен");
        // Сброс состояния, оповещения и т.д.
    }
    
    // Другие методы для управления глобальными состояниями боя
}
