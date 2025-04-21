using UnityEngine;
using System.Collections.Generic;
using System;

public class BattleMapManager : MonoBehaviour
{
    [Header("Настройка карты")]
    [SerializeField] private BattleConfig battleConfig;  // Ссылка на наш BattleConfig ScriptableObject

    [Header("Префабы для ячеек, персонажей, препятствий и т.п.")]
    public BattleCell hexPrefab;
    public GameObject[] prefabBattleCells; // Здесь можно добавить ссылки на префабы персонажей или препятствий

    // Константы для расчёта ячеек
    public const float outerToInner = 0.866025404f;
    public const float outerRadius = 1.1f;
    public const float innerRadius = outerRadius * outerToInner;
    public const float innerDiameter = innerRadius * 2;

    // Хранение ячеек
    private List<GameObject> hexCells = new List<GameObject>();
    GameObject parentObject; // Создание родительского объекта


    private int battleMapHeight;
    private int battleMapWidth;

    BattleCell[] battleCells;




    void Awake()
    {
        parentObject = new GameObject("HexGrid");
        parentObject.transform.SetParent(transform);

        battleMapHeight = battleConfig.battleMapSize.y;
        battleMapWidth = battleConfig.battleMapSize.x;

        CreateBattleMap(battleMapWidth, battleMapHeight);
    }

    void CreateBattleMap(int battleMapWidth,int battleMapHeight) // создание боевой карты
    {
        CreateBattleCells(battleMapWidth, battleMapHeight);
    }

    void CreateBattleCells(int battleMapWidth,int battleMapHeight)
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



}
