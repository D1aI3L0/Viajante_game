using UnityEngine;

public enum CellState
{
	Free,
	Ally,
	Enemy,
	Obstacle,
}

public class BattleCell : MonoBehaviour
{
	public int xСoordinate, zСoordinate; // координаты ячейки

	GameObject prefabBattleCell;
	public GameObject PrefabBattleCell
	{
		set
		{
			if (prefabBattleCell != null)
			{
				Destroy(prefabBattleCell);
			}
			prefabBattleCell = value;
			prefabBattleCell.transform.SetParent(transform, false);
		}
	}
	//======================================Состояние ячейки и объекты на ней===========================================

	[SerializeField] private CellState state = CellState.Free; // Основное состояние ячейки
	[SerializeField] private GameObject obstacleObject; // Ссылка на объект, находящийся на ячейке


	public bool IsWalkable => state == CellState.Free; // Удобное свойство для проверки доступности ячейки


	public CellState State
	{
		get => state;
		set => state = value;
	}

	public GameObject ObstacleObject
	{
		get => obstacleObject;
		set
		{
			obstacleObject = value;
			// Если устанавливается препятствие, обновляем состояние ячейки
			state = (obstacleObject != null) ? CellState.Obstacle : CellState.Free;
		}
	}

	// Дополнительные методы взаимодействия можно добавить здесь

	public void ClearObstacle()
	{
		obstacleObject = null;
		state = CellState.Free;
	}

	//==================================================Соседи==========================================================
	[SerializeField]
	BattleCell[] neighbors;

	public BattleCell GetNeighbor(HexDirection direction)
	{
		return neighbors[(int)direction];
	}

	public void SetNeighbor(HexDirection direction, BattleCell cell)
	{
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	public BattleCell[] GetNeighbors()
	{
		return neighbors;
	}
	//============================================================================================================












	// =============== Разработка =====================
	void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position, 0.1f);
		foreach (var neighbor in neighbors)
		{
			if (neighbor != null)
				Gizmos.DrawLine(transform.position, neighbor.transform.position);
		}
	}
	// ================================================
}
