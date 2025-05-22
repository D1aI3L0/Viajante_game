using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Mirror;

public class BattleMapManagerSP : MonoBehaviour
{
    public static BattleMapManagerSP Instance { get; private set; }

    [Header("Настройка карты")]
    public BattleConfig battleConfig;  // Ссылка на BattleConfig ScriptableObject

    [Header("Префабы ячеек, персонажей и препятствий")]
    public BattleCell hexPrefab;
    public GameObject[] prefabBattleCells;
    public GameObject obstaclePrefab;  // Префаб препятствия

    // Свойство для количества врагов (берется из EnemyTransferSystem)
    private int EnemyCount => enemyTransfer.enemiesCount;

    [Header("Префабы персонажей")]
    public GameObject warriorPrefab;
    public GameObject pathfinderPrefab;

    [Header("Данные персонажей для боя")]
    [SerializeField] private CharacterDataTransferParameters characterTransfer;
    [SerializeField] private EnemyTransferSystem enemyTransfer;

    public GameObject enemyPrefab; // Префаб вражеского персонажа (временно)

    // Словарь для сопоставления класса персонажа с префабом
    private Dictionary<CharacterClass, GameObject> prefabDictionary;

    // Константы для расчёта ячеек
    public const float outerToInner = 0.866025404f;
    public const float outerRadius = 1.1f;
    public const float innerRadius = outerRadius * outerToInner;
    public const float innerDiameter = innerRadius * 2;

    // Хранение ячеек и родительского объекта
    private List<GameObject> hexCells = new List<GameObject>();
    private GameObject parentObject;

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

        // Получаем размеры карты из BattleConfig (x — ширина, y — высота)
        battleMapWidth = battleConfig.battleMapSize.x;
        battleMapHeight = battleConfig.battleMapSize.y;

        // Определяем зоны: союзники будут занимать первые ряды, враги — последние
        allyMaxRow = Mathf.FloorToInt(battleMapHeight / 2f) - 1;
        enemyMinRow = Mathf.CeilToInt(battleMapHeight / 2f) + 1;

        characterTransfer.Transfer();
        enemyTransfer.Transfer();

        CreateBattleMap(battleMapWidth, battleMapHeight);
    }

    // Метод для тестовой инициализации (при необходимости)
    public void TestAwake()
    {
        Instance = this;

        hexPrefab.neighbors = new BattleCell[6];

        parentObject = new GameObject("HexGrid");
        parentObject.transform.SetParent(transform);

        battleMapWidth = battleConfig.battleMapSize.x;
        battleMapHeight = battleConfig.battleMapSize.y;

        allyMaxRow = Mathf.FloorToInt(battleMapHeight / 2f) - 1;
        enemyMinRow = Mathf.CeilToInt(battleMapHeight / 2f) + 1;
    }

    private void Start()
    {
        if (skipAutoInitialization)
            return;
        // После создания карты и установки препятствий — размещаем персонажей.
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

        // Внешний цикл по строкам (z), внутренний — по столбцам (x)
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

        BattleCell battleCell = battleCells[i] = Instantiate(hexPrefab);
        battleCell.transform.localPosition = position;
        battleCell.transform.SetParent(parentObject.transform);
        battleCell.name = $"BattleCell_{x}-{z}";

        battleCell.PrefabBattleCell = Instantiate(prefabBattleCells[0]);

        // Назначаем координаты для ячейки
        battleCell.xСoordinate = x;
        battleCell.zСoordinate = z;

        // Назначаем соседей
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
                    battleCell.SetNeighbor(HexDirection.SW, battleCells[i - battleMapWidth - 1]);
            }
            else
            {
                battleCell.SetNeighbor(HexDirection.SW, battleCells[i - battleMapWidth]);
                if (x < battleMapWidth - 1)
                    battleCell.SetNeighbor(HexDirection.SE, battleCells[i - battleMapWidth + 1]);
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
                availableCellIndices.Add(i);
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
    private void PlaceCharacters()
    {
        PlaceAllies();
        PlaceEnemies();
    }

    private void InitializePrefabDictionary()
    {
        prefabDictionary = new Dictionary<CharacterClass, GameObject>
        {
            { CharacterClass.WarriorZastupnik, warriorPrefab },
            { CharacterClass.Pathfinder, pathfinderPrefab }
            // Новые классы можно добавить здесь.
        };
    }

    // Возвращает нужный префаб по значению enum
    private GameObject GetPrefabForCharacter(CharacterClass characterClass)
    {
        if (prefabDictionary.TryGetValue(characterClass, out GameObject prefab))
            return prefab;
        else
        {
            Debug.LogWarning("Префаб для класса " + characterClass + " не назначен!");
            return null;
        }
    }

    private void PlaceAllies()
    {
        // Количество союзников задаётся в dataTransfer.numberOfCharacters
        for (int i = 0; i < characterTransfer.numberOfCharacters; i++)
        {
            BattleCell chosenCell = ChooseRandomCellForAlly();
            if (chosenCell != null)
            {
                // Получаем параметры персонажа для боя, чтобы определить его класс
                CharacterRuntimeParameters runtimeParams = characterTransfer.characters[i];
                GameObject prefab = GetPrefabForCharacter(runtimeParams.characterClass);
                if (prefab == null)
                {
                    Debug.LogWarning("Префаб для персонажа с классом " + runtimeParams.characterClass + " не найден.");
                    continue;
                }

                GameObject allyObj = Instantiate(prefab, chosenCell.transform.position, Quaternion.LookRotation(Vector3.forward));
                AllyBattleCharacterSP allyChar = allyObj.GetComponent<AllyBattleCharacterSP>();
                if (allyChar != null)
                {
                    allyChar.Init(runtimeParams);
                    // Регистрируем персонажа в менеджере боя и TurnManager
                    BattleManagerSP.Instance.RegisterAlly(allyChar);
                    TurnManagerSP.Instance.RegisterParticipant(allyChar);

                    chosenCell.SetOccupant(allyChar);
                    allyChar.CurrentCell = chosenCell;
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
            BattleCell chosenCell = ChooseRandomCellForEnemy();
            if (chosenCell != null)
            {
                EnemyStats enemyStats = enemyTransfer.enemyStats[i];
                GameObject enemyObj = Instantiate(enemyPrefab, chosenCell.transform.position, Quaternion.LookRotation(Vector3.back));
                EnemyBattleCharacterSP enemyChar = enemyObj.GetComponent<EnemyBattleCharacterSP>();
                if (enemyChar != null)
                {
                    enemyChar.Init(enemyStats);
                    // Регистрируем врага
                    BattleManagerSP.Instance.RegisterEnemy(enemyChar);
                    TurnManagerSP.Instance.RegisterParticipant(enemyChar);

                    chosenCell.SetOccupant(enemyChar);
                    enemyChar.CurrentCell = chosenCell;
                }
            }
        }
    }

    // Выбирает случайную свободную ячейку для союзника (например, в первой и второй строках)
    private BattleCell ChooseRandomCellForAlly()
    {
        List<BattleCell> candidateCells = new List<BattleCell>();
        foreach (BattleCell cell in battleCells)
        {
            if (cell.State == CellState.Free && cell.zСoordinate < 2)
                candidateCells.Add(cell);
        }
        if (candidateCells.Count > 0)
            return candidateCells[UnityEngine.Random.Range(0, candidateCells.Count)];
        else
            return null;
    }

    // Выбирает случайную свободную ячейку для врага (например, в последних двух строках)
    private BattleCell ChooseRandomCellForEnemy()
    {
        List<BattleCell> candidateCells = new List<BattleCell>();
        int enemyRowThreshold = battleMapHeight - 2;
        foreach (BattleCell cell in battleCells)
        {
            if (cell.State == CellState.Free && cell.zСoordinate >= enemyRowThreshold)
                candidateCells.Add(cell);
        }
        if (candidateCells.Count > 0)
            return candidateCells[UnityEngine.Random.Range(0, candidateCells.Count)];
        else
            return null;
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
            if (cell == null || !cell.IsWalkable)
                continue;

            int dx = Mathf.Abs(cell.xСoordinate - centerCell.xСoordinate);
            int dz = Mathf.Abs(cell.zСoordinate - centerCell.zСoordinate);
            if (dx <= radius && dz <= radius)
                cellsInRadius.Add(cell);
        }
        return cellsInRadius;
    }
}