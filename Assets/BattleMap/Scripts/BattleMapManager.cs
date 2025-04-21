using UnityEngine;
using System.Collections.Generic;
using System;

public class BattleMapManager : MonoBehaviour
{
    [Header("Настройка карты")]
    [SerializeField] private BattleConfig battleConfig;  // Ссылка на наш BattleConfig ScriptableObject

    [Header("Префабы для ячеек, персонажей, препятствий и т.п.")]
    public BattleCell hexPrefab;
    public GameObject[] prefabBattleCells;
    [SerializeField] private GameObject obstaclePrefab;  // Префаб препятствия

    // Константы для расчёта ячеек
    public const float outerToInner = 0.866025404f;
    public const float outerRadius = 1.1f;
    public const float innerRadius = outerRadius * outerToInner;
    public const float innerDiameter = innerRadius * 2;

    // Хранение ячеек
    private List<GameObject> hexCells = new List<GameObject>();
    private GameObject parentObject; // Создание родительского объекта


    private int battleMapHeight;
    private int battleMapWidth;
    private BattleCell[] battleCells;




    void Awake()
    {
        parentObject = new GameObject("HexGrid");
        parentObject.transform.SetParent(transform);

        // x - ширина (количество ячеек по горизонтали)  y - высота (по вертикали)
        battleMapWidth = battleConfig.battleMapSize.x;
        battleMapHeight = battleConfig.battleMapSize.y;

        CreateBattleMap(battleMapWidth, battleMapHeight);
    }

    void CreateBattleMap(int battleMapWidth, int battleMapHeight) // создание боевой карты
    {
        CreateBattleCells(battleMapWidth, battleMapHeight);
        PlaceObstacles();
    }

    void CreateBattleCells(int battleMapWidth, int battleMapHeight)
    {
        battleCells = new BattleCell[battleMapWidth * battleMapHeight];

        // Внешний цикл пробегает строки (вертикаль), то есть height
        for (int z = 0, i = 0; z < battleMapHeight; z++)
        {
            // Внутренний цикл пробегает столбцы (горизонталь), то есть width
            for (int x = 0; x < battleMapWidth; x++)
            {
                CreateBattleCell(x, z, i++);
            }
        }
    }

    void CreateBattleCell(int x, int z, int i) // создание боевой ячейки, назначение её соседей
    {
        Vector3 position;
        position.x = ((x + z * 0.5f - z / 2) * innerDiameter) + innerRadius;
        position.y = 0f;
        position.z = z * (outerRadius * 1.5f) + outerRadius;

        BattleCell battleCell = battleCells[i] = Instantiate<BattleCell>(hexPrefab);
        battleCell.transform.localPosition = position;
        battleCell.transform.SetParent(parentObject.transform);
        battleCell.name = $"BattleCell_{x}-{z}"; // Назначение имени с координатами

        battleCell.PrefabBattleCell = Instantiate<GameObject>(prefabBattleCells[0]);

        // задание координат
        battleCell.xСoordinate = x;
        battleCell.zСoordinate = z;



        // назначение соседей
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

    // Метод для размещения препятствий на карте
    void PlaceObstacles()
    {
        // Например, если BattleConfig содержит процент заполненности, 
        // рассчитываем количество ячеек, которые должны получить препятствие.
        // Пусть obstaclePercent — число от 0 до 1 (например, 0.05 для 5% заполненности)
        float obstaclePercent = battleConfig.obstaclePercent;  // поле в BattleConfig
        int totalCells = battleCells.Length;
        int obstaclesToPlace = Mathf.RoundToInt(totalCells * obstaclePercent);

        // Собираем список индексов ячеек, где можно разместить препятствие (состояние Free)
        List<int> availableCellIndices = new List<int>();
        for (int i = 0; i < totalCells; i++)
        {
            if (battleCells[i].State == CellState.Free)
            {
                availableCellIndices.Add(i);
            }
        }

        // Перемешиваем список для случайного выбора ячеек
        Shuffle(availableCellIndices);

        int placed = 0;
        // Проходим по перемешанному списку и размещаем препятствия при выполнении условия
        foreach (int index in availableCellIndices)
        {
            BattleCell cell = battleCells[index];

            // Простая проверка: будем размещать препятствие,
            // только если у ячейки есть по крайней мере один свободный сосед.
            bool hasFreeNeighbor = false;
            // Предполагается, что у всех ячеек массив neighbors инициализирован
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
                // Создаём объект препятствия и привязываем его к ячейке
                GameObject obstacleInstance = Instantiate(obstaclePrefab, cell.transform.position, Quaternion.identity, cell.transform);
                cell.ObstacleObject = obstacleInstance;
                placed++;
                if (placed >= obstaclesToPlace)
                    break;
            }
        }
    }

    // Пример стандартного алгоритма перемешивания списка (Fisher-Yates)
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

}
