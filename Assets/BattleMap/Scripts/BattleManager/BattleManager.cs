using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    // Конфигурация боя, в которой указан тип игры
    public BattleConfig battleConfig;

    [Header("Префабы менеджеров для одиночного режима")]
    public GameObject battleMapManagerSPPrefab;
    public GameObject turnManagerSPPrefab;
    public GameObject battleManagerSPPrefab;

    [Header("Префабы менеджеров для многопользовательского режима")]
    public GameObject battleMapManagerMPPrefab;
    public GameObject turnManagerMPPrefab;
    public GameObject battleManagerMPPrefab;

    private void Awake()
    {
        // Реализация синглтона
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        

        // В зависимости от настроек битвы создаём нужные объекты менеджеров
        CreateManagers();
    }

    /// <summary>
    /// Метод инициализирует менеджеры в зависимости от типа игры (одиночный или мультиплеер)
    /// </summary>
    private void CreateManagers()
    {
        if (battleConfig == null)
        {
            Debug.LogError("BattleConfig не назначен в BattleManager");
            return;
        }

        if (BattleConfig.IsMultiplayer)
        {
            Debug.Log("Запуск в многопользовательском режиме – создаём MP-менеджеры.");
            // Создаем Multiplayer версию менеджеров как дочерние объекты BattleManager
            if (battleMapManagerMPPrefab != null)
                Instantiate(battleMapManagerMPPrefab);
            else
                Debug.LogWarning("Префаб BattleMapManagerMP не назначен!");

            if (turnManagerMPPrefab != null)
                Instantiate(turnManagerMPPrefab, transform.position, Quaternion.identity, transform);
            else
                Debug.LogWarning("Префаб TurnManagerMP не назначен!");

            if (battleManagerMPPrefab != null)
                Instantiate(battleManagerMPPrefab, transform.position, Quaternion.identity, transform);
            else
                Debug.LogWarning("Префаб BattleManagerMP не назначен!");


            // При необходимости здесь можно создать и NetworkManager, если он несоздан из другого места.
        }
        else
        {
            Debug.Log("Запуск в одиночном режиме – создаём SP-менеджеры.");
            // Создаем SinglePlayer версию менеджеров
            if (battleMapManagerSPPrefab != null)
                Instantiate(battleMapManagerSPPrefab);
            else
                Debug.LogWarning("Префаб BattleMapManagerSP не назначен!");

            if (turnManagerSPPrefab != null)
                Instantiate(turnManagerSPPrefab, transform.position, Quaternion.identity, transform);
            else
                Debug.LogWarning("Префаб TurnManagerSP не назначен!");

            if (battleManagerSPPrefab != null)
                Instantiate(battleManagerSPPrefab, transform.position, Quaternion.identity, transform);
            else
                Debug.LogWarning("Префаб BattleManagerSP не назначен!");
        }
    }
}