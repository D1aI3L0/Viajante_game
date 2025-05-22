#if MIRROR
using Mirror;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class BattleMapManagerMP : NetworkBehaviour
{
    public static BattleMapManagerMP Instance { get; private set; }

    [Header("Настройка карты")]
    public BattleConfig battleConfig;  // Ссылка на BattleConfig ScriptableObject

    [Header("Префабы ячеек, персонажей и препятствий")]
    public BattleCell hexPrefab;
    public GameObject[] prefabBattleCells;
    public GameObject obstaclePrefab;  // Префаб препятствия

    private int EnemyCount => enemyTransfer.enemiesCount; // Количество противников

    [Header("Префабы персонажей")]
    public GameObject warriorPrefab;
    public GameObject pathfinderPrefab;

    [Header("Данные персонажей для боя")]
    [SerializeField] private CharacterDataTransferParameters dataTransfer;
    [SerializeField] private EnemyTransferSystem enemyTransfer;

    public GameObject enemyPrefab; // Префаб вражеского персонажа (временно)

    private Dictionary<CharacterClass, GameObject> prefabDictionary;

    // Константы для расчёта ячеек
    public const float outerToInner = 0.866025404f;
    public const float outerRadius = 1.1f;
    public const float innerRadius = outerRadius * outerToInner;
    public const float innerDiameter = innerRadius * 2;

    // Хранение ячеек и родительский объект
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

        battleMapWidth = battleConfig.battleMapSize.x;
        battleMapHeight = battleConfig.battleMapSize.y;

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

        battleMapWidth = battleConfig.battleMapSize.x;
        battleMapHeight = battleConfig.battleMapSize.y;

        allyMaxRow = Mathf.FloorToInt(battleMapHeight / 2f) - 1;
        enemyMinRow = Mathf.CeilToInt(battleMapHeight / 2f) + 1;
    }

    private void Start()
    {
        if (skipAutoInitialization)
            return;
        // Размещаем персонажей только на сервере
        if (isServer)
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

        battleCell.xСoordinate = x;
        battleCell.zСoordinate = z;

        if (x > 0)
            battleCell.SetNeighbor(HexDirection.W, battleCells[i - 1]);
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

    private void PlaceCharacters()
    {
        if (isServer)
        {
            PlaceAllies();
            PlaceEnemies();
        }
    }

    private void InitializePrefabDictionary()
    {
        prefabDictionary = new Dictionary<CharacterClass, GameObject>
        {
            { CharacterClass.WarriorZastupnik, warriorPrefab },
            { CharacterClass.Pathfinder, pathfinderPrefab }
            // Добавляем новые классы по необходимости.
        };
    }

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
        for (int i = 0; i < dataTransfer.numberOfCharacters; i++)
        {
            BattleCell chosenCell = ChooseRandomCellForAlly();
            if (chosenCell != null)
            {
                CharacterRuntimeParameters runtimeParams = dataTransfer.characters[i];
                GameObject prefab = GetPrefabForCharacter(runtimeParams.characterClass);
                if (prefab == null)
                {
                    Debug.LogWarning("Префаб для персонажа с классом " + runtimeParams.characterClass + " не найден.");
                    continue;
                }
                GameObject allyObj = Instantiate(prefab, chosenCell.transform.position, Quaternion.LookRotation(Vector3.forward));
                AllyBattleCharacterMP allyChar = allyObj.GetComponent<AllyBattleCharacterMP>();
                if (allyChar != null)
                {
                    allyChar.Init(runtimeParams);
                    BattleManagerMP.Instance.RegisterAlly(allyChar);
                    TurnManagerMP.Instance.RegisterParticipant(allyChar);

                    chosenCell.SetOccupant(allyChar);
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
            BattleCell chosenCell = ChooseRandomCellForEnemy();
            if (chosenCell != null)
            {
                EnemyStats enemyStats = enemyTransfer.enemyStats[i];
                GameObject enemyObj = Instantiate(enemyPrefab, chosenCell.transform.position, Quaternion.LookRotation(Vector3.back));
                EnemyBattleCharacterMP enemyChar = enemyObj.GetComponent<EnemyBattleCharacterMP>();
                if (enemyChar != null)
                {
                    enemyChar.Init(enemyStats);
                    BattleManagerMP.Instance.RegisterEnemy(enemyChar);
                    TurnManagerMP.Instance.RegisterParticipant(enemyChar);

                    chosenCell.SetOccupant(enemyChar);
                    enemyChar.currentCell = chosenCell;
                }
            }
        }
    }

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

    [Server]
    public void ServerGenerateMap()
    {
        CreateBattleMap();
        List<int> obstacleIndices = new List<int>();
        for (int i = 0; i < battleCells.Length; i++)
        {
            if (battleCells[i].State == CellState.Obstacle)
                obstacleIndices.Add(i);
        }
        RpcSyncMap(obstacleIndices.ToArray(), battleMapWidth, battleMapHeight);
    }

    [ClientRpc]
    private void RpcSyncMap(int[] obstacleIndices, int width, int height)
    {
        battleMapWidth = width;
        battleMapHeight = height;
        CreateEmptyGrid();
        foreach (int index in obstacleIndices)
        {
            battleCells[index].SetObstacle(obstaclePrefab);
        }
    }

    [Server]
    public void ServerPlaceCharacter(BattleEntityMP entity, int x, int z)
    {
        BattleCell cell = GetCell(x, z);
        if (cell != null && cell.IsWalkable)
        {
            cell.SetOccupant(entity);
            entity.GetComponent<NetworkTransformReliable>().ServerTeleport(cell.transform.position, new());
        }
    }

    [Server]
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

    [Server]
    public Vector2[] GetSpawnZones()
    {
        // Возвращаем зоны спавна для союзников и врагов:
        // Союзники: от z = 0 до allyMaxRow, враги: от enemyMinRow до battleMapHeight - 1.
        return new Vector2[]
        {
            new Vector2(0, allyMaxRow),
            new Vector2(enemyMinRow, battleMapHeight)
        };
    }

    [Server]
    public void RecreateMap(int[] obstacleIndices, Vector2[] spawnZones)
    {
        if (battleCells != null)
        {
            foreach (BattleCell cell in battleCells)
            {
                if (cell != null)
                    Destroy(cell.gameObject);
            }
            battleCells = null;
        }
        CreateBattleCells(battleMapWidth, battleMapHeight);
        foreach (int index in obstacleIndices)
        {
            if (index >= 0 && index < battleCells.Length)
            {
                GameObject obstacle = Instantiate(obstaclePrefab, battleCells[index].transform.position, Quaternion.identity);
                battleCells[index].ObstacleObject = obstacle;
            }
        }
        allyMaxRow = (int)spawnZones[0].y;
        enemyMinRow = (int)spawnZones[1].x;
    }

    [Server]
    public BattleCell GetFreeSpawnCell(bool isAlly = true)
    {
        List<BattleCell> candidateCells = new List<BattleCell>();
        int minZ = isAlly ? 0 : enemyMinRow;
        int maxZ = isAlly ? allyMaxRow : battleMapHeight - 1;

        foreach (var cell in battleCells)
        {
            if (cell == null || cell.State != CellState.Free)
                continue;
            if (cell.zСoordinate >= minZ && cell.zСoordinate <= maxZ)
                candidateCells.Add(cell);
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
        battleCells = new BattleCell[battleMapWidth * battleMapHeight];
        for (int i = 0; i < battleCells.Length; i++)
        {
            battleCells[i] = Instantiate(hexPrefab, Vector3.zero, Quaternion.identity);
            battleCells[i].SetState(CellState.Free);
        }
    }
}
#endif