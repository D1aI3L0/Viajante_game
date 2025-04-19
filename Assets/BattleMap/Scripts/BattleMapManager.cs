using UnityEngine;
using System.Collections.Generic;

public class BattleMapManager : MonoBehaviour
{
    public BattleCell hexPrefab;
    public int battleMapHeight = 5; // Z высота
    public int battleMapWidth = 5; // X ширина
    public const float outerToInner = 0.866025404f;
    public const float outerRadius = 1.1f;
    public const float innerRadius = outerRadius * outerToInner;
    public const float innerDiameter = innerRadius * 2;
    private List<GameObject> hexCells = new List<GameObject>(); // Список для хранения ячеек
    GameObject parentObject; // Создание родительского объекта

    BattleCell[] battleCells;

    [SerializeField]
	GameObject[] prefabBattleCells;


    void Awake()
    {
        parentObject = new GameObject("HexGrid");
        parentObject.transform.SetParent(transform);
        CreateBattleMap();
    }

    void CreateBattleMap() // создание боевой карты
    {
        CreateBattleCells();
    }

    void CreateBattleCells() 
    {
        battleCells = new BattleCell[battleMapWidth * battleMapHeight]; // создание массива ячеек

        for (int z = 0, i = 0; z < battleMapWidth; z++) 
        {
            for (int x = 0; x < battleMapHeight; x++)
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
            else {
				battleCell.SetNeighbor(HexDirection.SW, battleCells[i - battleMapWidth]);
				if (x < battleMapWidth - 1) {
					battleCell.SetNeighbor(HexDirection.SE, battleCells[i - battleMapWidth + 1]);
				}
			}
		}
    }



}
