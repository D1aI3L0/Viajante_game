using UnityEngine;

public class BattleCell : MonoBehaviour
{
	public int xСoordinate;
	public int zСoordinate;
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

	void Start()
	{

	}

	//============================================================================================================
	//                                              Соседи 
	//============================================================================================================
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
