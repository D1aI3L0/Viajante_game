#if MIRROR
using Mirror;
#endif
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

[Serializable]
public class BattleMapManager :
#if MIRROR
    NetworkBehaviour
#else
    MonoBehaviour
#endif
{
    public static BattleMapManager Instance { get; private set; }

    [Header("Настройка карты")]
    public BattleConfig battleConfig;  // Ссылка на BattleConfig ScriptableObject

    [Header("Префабы ячеек, персонажей и препятствий")]
    public BattleCell hexPrefab;
    public GameObject[] prefabBattleCells;
    public GameObject obstaclePrefab;  // Префаб препятствия

    private int EnemyCount => enemyTransfer.enemiesCount; // свойство количество противников

    [Header("Префабы персонажей")]
    public GameObject warriorPrefab;
    public GameObject pathfinderPrefab;


    [Header("Данные персонажей для боя")]
    [SerializeField] private CharacterDataTransferParameters dataTransfer;
    [SerializeField] private EnemyTransferSystem enemyTransfer;


    public GameObject enemyPrefab; // Префаб вражеского персонажа (временно)

    private Dictionary<CharacterClass, GameObject> prefabDictionary; // Словарь для сопоставления класса персонажа и префаба


    // Константы для расчёта ячеек
    public const float outerToInner = 0.866025404f;
    public const float outerRadius = 1.1f;
    public const float innerRadius = outerRadius * outerToInner;
    public const float innerDiameter = innerRadius * 2;

    // Хранение ячеек
    private List<GameObject> hexCells = new List<GameObject>();
    private GameObject parentObject; // Родительский объект для ячеек

    private int battleMapHeight;
    private int battleMapWidth;
    private BattleCell[] battleCells;

    // Параметры зон для размещения персонажей
    private int allyMaxRow;   // Максимальный z для союзной зоны
    private int enemyMinRow;  // Минимальный z для вражеской зоны

    internal bool skipAutoInitialization = false;

    private void Awake()
    {
        if (skipAutoInitialization)
            return;
            
        Instance = this;
        InitializePrefabDictionary();

        parentObject = new GameObject("HexGrid");
        parentObject.transform.SetParent(transform);

        // Получаем размеры карты из BattleConfig (x - ширина, y - высота)
        battleMapWidth = battleConfig.battleMapSize.x;
        battleMapHeight = battleConfig.battleMapSize.y;

        // Определяем зоны: например, союзники занимают первые ряды, враги — последние
        // Можно оставить промежуток между ними, изменив формулу
        allyMaxRow = Mathf.FloorToInt(battleMapHeight / 2f) - 1;
        enemyMinRow = Mathf.CeilToInt(battleMapHeight / 2f) + 1;

        CreateBattleMap(battleMapWidth, battleMapHeight);
    }

    public void TestAwake()
    {
        Instance = this;

        hexPrefab.neighbors = new BattleCell[6];

        parentObject = new GameObject("HexGrid");
        parentObject.transform.SetParent(transform);

        // Получаем размеры карты из BattleConfig (x - ширина, y - высота)
        battleMapWidth = battleConfig.battleMapSize.x;
        battleMapHeight = battleConfig.battleMapSize.y;

        // Определяем зоны: например, союзники занимают первые ряды, враги — последние
        // Можно оставить промежуток между ними, изменив формулу
        allyMaxRow = Mathf.FloorToInt(battleMapHeight / 2f) - 1;
        enemyMinRow = Mathf.CeilToInt(battleMapHeight / 2f) + 1;
    }

    public void Start()
    {
        if (skipAutoInitialization)
            return;
        // После создания карты и установки препятствий помещаем персонажей
        PlaceCharacters();
    }

    public void CreateBattleMap()
    {
        CreateBattleMap(battleMapWidth, battleMapHeight);
    }

    private void CreateBattleMap(int battleMapWidth, int battleMapHeight)
    {
        CreateBattleCells(battleMapWidth, battleMapHeight);
        PlaceObstacles();
    }

    private void CreateBattleCells(int battleMapWidth, int battleMapHeight)
    {
        battleCells = new BattleCell[battleMapWidth * battleMapHeight];

        // Внешний цикл по строкам (z), внутренний по столбцам (x)
        for (int z = 0, i = 0; z < battleMapHeight; z++)
        {
            for (int x = 0; x < battleMapWidth; x++)
            {
                CreateBattleCell(x, z, i++);
            }
        }
    }

    private void CreateBattleCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = ((x + z * 0.5f - z / 2) * innerDiameter) + innerRadius;
        position.y = 0f;
        position.z = z * (outerRadius * 1.5f) + outerRadius;

        BattleCell battleCell = battleCells[i] = Instantiate<BattleCell>(hexPrefab);
        battleCell.transform.localPosition = position;
        battleCell.transform.SetParent(parentObject.transform);
        battleCell.name = $"BattleCell_{x}-{z}";

        battleCell.PrefabBattleCell = Instantiate<GameObject>(prefabBattleCells[0]);

        // Назначение координат
        battleCell.xСoordinate = x;
        battleCell.zСoordinate = z;

        // Назначение соседей
        if (x > 0)
        {
            battleCell.SetNeighbor(HexDirection.W, battleCells[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                battleCell.SetNeighbor(HexDirection.SE, battleCells[i - battleMapWidth]);
                if (x > 0)
                {
                    battleCell.SetNeighbor(HexDirection.SW, battleCells[i - battleMapWidth - 1]);
                }
            }
            else
            {
                battleCell.SetNeighbor(HexDirection.SW, battleCells[i - battleMapWidth]);
                if (x < battleMapWidth - 1)
                {
                    battleCell.SetNeighbor(HexDirection.SE, battleCells[i - battleMapWidth + 1]);
                }
            }
        }
    }

    private void PlaceObstacles()
    {
        float obstaclePercent = battleConfig.obstaclePercent;
        int totalCells = battleCells.Length;
        int obstaclesToPlace = Mathf.RoundToInt(totalCells * obstaclePercent);

        List<int> availableCellIndices = new List<int>();
        for (int i = 0; i < totalCells; i++)
        {
            if (battleCells[i].State == CellState.Free)
            {
                availableCellIndices.Add(i);
            }
        }
        Shuffle(availableCellIndices);

        int placed = 0;
        foreach (int index in availableCellIndices)
        {
            BattleCell cell = battleCells[index];

            bool hasFreeNeighbor = false;
            foreach (var neighbor in cell.GetNeighbors())
            {
                if (neighbor != null && neighbor.State == CellState.Free)
                {
                    hasFreeNeighbor = true;
                    break;
                }
            }

            if (hasFreeNeighbor)
            {
                GameObject obstacleInstance = Instantiate(obstaclePrefab, cell.transform.position, Quaternion.identity, cell.transform);
                cell.ObstacleObject = obstacleInstance;
                placed++;
                if (placed >= obstaclesToPlace)
                    break;
            }
        }
    }

    // Стандартный алгоритм перемешивания (Fisher-Yates)
    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    // -------------------------
    // Размещение персонажей
    // -------------------------
    // Метод для размещения персонажей – вызывается из PlaceCharacters()
    private void PlaceCharacters()
    {
#if MIRROR
        if (isServer)
        {
            PlaceAllies();
            PlaceEnemies();
        }
#else
        PlaceAllies();
        PlaceEnemies();
#endif
    }

    private void InitializePrefabDictionary()
    {
        prefabDictionary = new Dictionary<CharacterClass, GameObject>
        {
            { CharacterClass.WarriorZastupnik, warriorPrefab },
            { CharacterClass.Pathfinder, pathfinderPrefab }
            // Если появятся новые классы, их можно добавить сюда:
            // { CharacterClass.Mage, magePrefab }
        };
    }

    // Метод, который возвращает нужный префаб по значению enum
    private GameObject GetPrefabForCharacter(CharacterClass characterClass)
    {
        if (prefabDictionary.TryGetValue(characterClass, out GameObject prefab))
        {
            return prefab;
        }
        else
        {
            Debug.LogWarning("Префаб для класса " + characterClass + " не назначен!");
            return null;
        }
    }

    private void PlaceAllies()
    {
        // Количество союзников задаётся в dataTransfer.numberOfCharacters
        for (int i = 0; i < dataTransfer.numberOfCharacters; i++)
        {
            BattleCell chosenCell = ChooseRandomCellForAlly();
            if (chosenCell != null)
            {
                // Получаем runtime-параметры персонажа, чтобы узнать его класс
                CharacterRuntimeParameters runtimeParams = dataTransfer.characters[i];
                GameObject prefab = GetPrefabForCharacter(runtimeParams.characterClass);

                if (prefab == null)
                {
                    Debug.LogWarning("Префаб для персонажа с классом " + runtimeParams.characterClass + " не найден.");
                    continue;
                }

                // Создаём экземпляр объекта
                GameObject allyObj = Instantiate(prefab, chosenCell.transform.position, Quaternion.LookRotation(Vector3.forward));

                // Инициализируем экземпляр, если у него есть компонент AllyBattleCharacter
                AllyBattleCharacter allyChar = allyObj.GetComponent<AllyBattleCharacter>();
                if (allyChar != null)
                {
                    allyChar.Init(runtimeParams);
                    // Регистрируем юнита в BattleManager
                    BattleManager.Instance.RegisterAlly(allyChar);
                    // Регистрируем юнита в TurnManager
                    TurnManager.Instance.RegisterParticipant(allyChar);

                    // Устанавливаем персонажа в выбранную клетку:
                    chosenCell.SetOccupant(allyChar);
                    // Если метод SetOccupant не устанавливает currentCell, то можно ещё явно:
                    allyChar.currentCell = chosenCell;
                }
            }
            else
            {
                Debug.LogWarning("Нет свободной ячейки для союзника");
            }
        }
    }


    private void PlaceEnemies()
    {
        for (int i = 0; i < EnemyCount; i++)
        {
            // Выбираем ячейку для врага
            BattleCell chosenCell = ChooseRandomCellForEnemy();
            if (chosenCell != null)
            {
                EnemyStats enemyStats = enemyTransfer.enemyStats[i];

                // Создаём экземпляр врага
                GameObject enemyObj = Instantiate(enemyPrefab, chosenCell.transform.position, Quaternion.LookRotation(Vector3.back));

                // Получаем компонент EnemyBattleCharacter
                EnemyBattleCharacter enemyChar = enemyObj.GetComponent<EnemyBattleCharacter>();
                if (enemyChar != null)
                {
                    enemyChar.Init(enemyStats);
                    // Регистрируем врага в BattleManager
                    BattleManager.Instance.RegisterEnemy(enemyChar);
                    // Регистрируем врага в TurnManager
                    TurnManager.Instance.RegisterParticipant(enemyChar);

                    chosenCell.SetOccupant(enemyChar);
                    enemyChar.currentCell = chosenCell;
                }
            }
        }
    }


    // Выбирает случайную свободную ячейку для союзника среди ячеек в первых двух рядах (z = 0 и 1)
    private BattleCell ChooseRandomCellForAlly()
    {
        List<BattleCell> candidateCells = new List<BattleCell>();

        foreach (BattleCell cell in battleCells)
        {
            // Предположим, что первые два ряда (z = 0 и z = 1) задают зону союзников.
            if (cell.State == CellState.Free && cell.zСoordinate < 2)
            {
                candidateCells.Add(cell);
            }
        }

        if (candidateCells.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidateCells.Count);
            return candidateCells[randomIndex];
        }
        else
        {
            return null;
        }
    }

    // Выбирает случайную свободную ячейку для врага среди ячеек в последних двух рядах
    private BattleCell ChooseRandomCellForEnemy()
    {
        List<BattleCell> candidateCells = new List<BattleCell>();

        // Пусть высота карты — battleMapHeight; для врагов используем последние два ряда: battleMapHeight - 2 и battleMapHeight - 1.
        int enemyRowThreshold = battleMapHeight - 2;
        foreach (BattleCell cell in battleCells)
        {
            if (cell.State == CellState.Free && cell.zСoordinate >= enemyRowThreshold)
            {
                candidateCells.Add(cell);
            }
        }

        if (candidateCells.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidateCells.Count);
            return candidateCells[randomIndex];
        }
        else
        {
            return null;
        }
    }

    internal BattleCell GetCell(int targetX, int targetZ)
    {
        for (int i = 0; i < battleCells.Length; i++)
        {
            if (battleCells[i] != null && battleCells[i].xСoordinate == targetX && battleCells[i].zСoordinate == targetZ)
                return battleCells[i];
        }
        return null;
    }

    public BattleCell[] GetAllCells()
    {
        if (battleCells == null)
        {
            Debug.LogWarning("Карта ещё не инициализирована!");
            return new BattleCell[0];
        }
        return battleCells.Where(cell => cell != null).ToArray();
    }

    public List<BattleCell> GetCellsInRadius(BattleCell centerCell, int radius)
    {
        List<BattleCell> cellsInRadius = new List<BattleCell>();
        foreach (var cell in battleCells)
        {
            if (cell == null || !cell.IsWalkable) continue;

            // Рассчитываем расстояние между клетками
            int dx = Mathf.Abs(cell.xСoordinate - centerCell.xСoordinate);
            int dz = Mathf.Abs(cell.zСoordinate - centerCell.zСoordinate);
            if (dx <= radius && dz <= radius)
            {
                cellsInRadius.Add(cell);
            }
        }
        return cellsInRadius;
    }

#if MIRROR
    [Server]
    public void ServerGenerateMap()
    {
        // 1. Генерация карты на сервере
        CreateBattleMap();

        // 2. Собираем индексы ячеек с препятствиями
        List<int> obstacleIndices = new List<int>();
        for (int i = 0; i < battleCells.Length; i++)
        {
            if (battleCells[i].State == CellState.Obstacle)
                obstacleIndices.Add(i);
        }

        // 3. Отправляем данные клиентам
        RpcSyncMap(
            obstacleIndices.ToArray(),
            battleMapWidth,
            battleMapHeight
        );
    }

    [ClientRpc]
    private void RpcSyncMap(int[] obstacleIndices, int width, int height)
    {
        // Клиенты воссоздают карту
        battleMapWidth = width;
        battleMapHeight = height;
        CreateEmptyGrid();

        // Добавляем препятствия
        foreach (int index in obstacleIndices)
        {
            battleCells[index].SetObstacle(obstaclePrefab);
        }
    }
#endif

#if MIRROR
    [Server]
    public void ServerPlaceCharacter(BattleEntity entity, int x, int z)
    {
        BattleCell cell = GetCell(x, z);
        if (cell != null && cell.IsWalkable)
        {
            // Размещаем на сервере
            cell.SetOccupant(entity);
            // Синхронизируем позицию
            entity.GetComponent<NetworkTransformReliable>().ServerTeleport(cell.transform.position, new());
        }
    }
#endif

#if MIRROR
    public int[] GetObstacleIndices()
    {
        List<int> indices = new List<int>();
        for (int i = 0; i < battleCells.Length; i++)
        {
            if (battleCells[i].State == CellState.Obstacle)
                indices.Add(i);
        }
        return indices.ToArray();
    }

    public Vector2[] GetSpawnZones()
    {
        // Возвращаем зоны спавна для союзников и врагов
        // Пример: 
        // - Союзники: z < allyMaxRow
        // - Враги: z >= enemyMinRow
        return new Vector2[]
        {
        new Vector2(0, allyMaxRow),          // Зона союзников (minZ, maxZ)
        new Vector2(enemyMinRow, battleMapHeight) // Зона врагов
        };
    }

    public void RecreateMap(int[] obstacleIndices, Vector2[] spawnZones)
    {
        // Очищаем текущую карту
        if (battleCells != null)
        {
            foreach (BattleCell cell in battleCells)
            {
                if (cell != null) Destroy(cell.gameObject);
            }
            battleCells = null;
        }

        // Создаём пустую сетку
        CreateBattleCells(battleMapWidth, battleMapHeight);

        // Восстанавливаем препятствия
        foreach (int index in obstacleIndices)
        {
            if (index >= 0 && index < battleCells.Length)
            {
                GameObject obstacle = Instantiate(obstaclePrefab, battleCells[index].transform.position, Quaternion.identity);
                battleCells[index].ObstacleObject = obstacle;
            }
        }

        // Обновляем зоны спавна (если нужно)
        allyMaxRow = (int)spawnZones[0].y;
        enemyMinRow = (int)spawnZones[1].x;
    }

    public BattleCell GetFreeSpawnCell(bool isAlly = true)
    {
        List<BattleCell> candidateCells = new List<BattleCell>();
        int minZ = isAlly ? 0 : enemyMinRow;
        int maxZ = isAlly ? allyMaxRow : battleMapHeight - 1;

        foreach (var cell in battleCells)
        {
            if (cell == null || cell.State != CellState.Free) continue;
            if (cell.zСoordinate >= minZ && cell.zСoordinate <= maxZ)
            {
                candidateCells.Add(cell);
            }
        }

        if (candidateCells.Count == 0)
        {
            Debug.LogWarning("Нет свободных ячеек для спавна!");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0, candidateCells.Count);
        return candidateCells[randomIndex];
    }

    private void CreateEmptyGrid()
    {
        // Создаёт пустую сетку ячеек без препятствий
        battleCells = new BattleCell[battleMapWidth * battleMapHeight];
        for (int i = 0; i < battleCells.Length; i++)
        {
            battleCells[i] = Instantiate(hexPrefab, Vector3.zero, Quaternion.identity);
            battleCells[i].SetState(CellState.Free);
        }
    }
#endif
}