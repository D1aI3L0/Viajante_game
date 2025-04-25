using UnityEngine;
using System.Collections.Generic;
using System;

public class BattleMapManager : MonoBehaviour
{
    [Header("Настройка карты")]
    [SerializeField] private BattleConfig battleConfig;  // Ссылка на BattleConfig ScriptableObject

    [Header("Префабы ячеек, персонажей и препятствий")]
    public BattleCell hexPrefab;
    public GameObject[] prefabBattleCells;
    [SerializeField] private GameObject obstaclePrefab;  // Префаб препятствия

    [Header("Настройка персонажей")]
    private int AllyCount => dataTransfer != null ? dataTransfer.numberOfCharacters : 0; // свойство кол-во юнитов игрока
    private int EnemyCount => battleConfig.enemyCount; // свойство количество противников
    public GameObject allyPrefab; // Префаб союзного персонажа (например, Воин защитник)
    public GameObject enemyPrefab; // Префаб вражеского персонажа (можно временно использовать тот же)

    [Header("Данные персонажей для боя")]
    [SerializeField] private CharacterDataTransferParameters dataTransfer;


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

    void Awake()
    {
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

    void Start()
    {
        // После создания карты и установки препятствий помещаем персонажей
        PlaceCharacters();
    }

    void CreateBattleMap(int battleMapWidth, int battleMapHeight)
    {
        CreateBattleCells(battleMapWidth, battleMapHeight);
        PlaceObstacles();
    }

    void CreateBattleCells(int battleMapWidth, int battleMapHeight)
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

    void CreateBattleCell(int x, int z, int i)
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

    void PlaceObstacles()
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
    void Shuffle<T>(List<T> list)
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
    void PlaceCharacters()
    {
        PlaceAllies();
        PlaceEnemies();
    }

    // Подразумевается, что ты добавил в BattleMapManager:
    // [Header("Данные персонажей для боя")]
    // [SerializeField] private CharacterDataTransferParameters dataTransfer;

    void PlaceAllies()
    {
        for (int i = 0; i < AllyCount; i++)
        {
            BattleCell chosenCell = ChooseRandomCellForAlly();
            if (chosenCell != null)
            {
                // Создаем префаб союзника в позиции выбранной ячейки.
                GameObject allyObj = Instantiate(allyPrefab, chosenCell.transform.position, Quaternion.LookRotation(Vector3.forward));

                // Получаем компонент BattleCharacter на созданном объекте
                BattleCharacter bc = allyObj.GetComponent<BattleCharacter>();

                // Если компонент найден и есть данные для этого персонажа, инициализируем его данные:
                // Предполагаем, что runtime данные хранятся в массиве characters, и порядок соответствует порядку размещения.
                if (bc != null && dataTransfer != null && dataTransfer.characters.Length > i)
                {
                    bc.Init(dataTransfer.characters[i]);
                }
                else
                {
                    Debug.LogWarning("BattleCharacter не найден или данные отсутствуют");
                }

                // После успешного размещения меняем состояние ячейки на занятую союзником.
                chosenCell.State = CellState.Ally;
            }
            else
            {
                Debug.LogWarning("Нет свободной ячейки для союзника");
            }
        }
    }

    void PlaceEnemies()
    {
        for (int i = 0; i < EnemyCount; i++)
        {
            // Используем метод для врагов, чтобы выбрать ячейку из последних дву рядов
            BattleCell chosenCell = ChooseRandomCellForEnemy();
            if (chosenCell != null)
            {
                // Создаем префаб врага в позиции выбранной ячейки и ориентируем его так, чтобы он смотрел в сторону союзников
                GameObject enemyObj = Instantiate(enemyPrefab, chosenCell.transform.position, Quaternion.LookRotation(Vector3.back));

                // Получаем компонент BattleCharacter на созданном объекте
                BattleCharacter bc = enemyObj.GetComponent<BattleCharacter>();

                // После успешного размещения меняем состояние ячейки на занятую врагом.
                chosenCell.State = CellState.Enemy;
            }
            else
            {
                Debug.LogWarning("Нет свободной ячейки для врагов");
            }
        }
    }

    // Выбирает случайную свободную ячейку для союзника среди ячеек в первых двух рядах (z = 0 и 1)
    BattleCell ChooseRandomCellForAlly()
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
    BattleCell ChooseRandomCellForEnemy()
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

}
