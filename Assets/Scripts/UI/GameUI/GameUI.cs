using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameUI : MonoBehaviour
{
	public static GameUI Instance;
	public HexGrid grid;
	HexCell currentCell;
	public Unit selectedUnit;
	public GameObject UIContainer;

	public TMP_Text currentTurnLable;
	public int CurrentTurn
	{
		set
		{
			selectedUnit = null;
			currentCell = null;
			currentTurnLable.text = value.ToString();
		}
	}

    void Awake()
    {
		Instance = this;
        Toggle(true);
    }

    void Update()
	{
		if (!EventSystem.current.IsPointerOverGameObject())
		{
			if (Input.GetMouseButtonDown(0))
			{
				DoSelection();
			}
			if (selectedUnit is Squad squad)
			{
				if (squad.squadType == SquadType.Player)
				{
					if (Input.GetMouseButtonDown(1))
					{
						DoMove();
					}
					else
					{
						DoPathfinding();
					}
				}
				else
				{

				}
			}
			else if (selectedUnit is Base @base)
			{
				if (Input.GetMouseButtonDown(1))
				{
					DoMove();
				}
				else
				{
					DoPathfinding();
				}
			}
			else
				grid.ClearPath();
		}
	}

	public void Toggle(bool toggle)
	{
		grid.ShowUI(toggle);
		grid.ClearPath();
		UIContainer.SetActive(toggle);

		if (!toggle)
		{
			Shader.EnableKeyword("HEX_MAP_EDIT_MODE");
			selectedUnit = null;
		}
		else
		{
			Shader.DisableKeyword("HEX_MAP_EDIT_MODE");
		}
	}

	bool UpdateCurrentCell()
	{
		HexCell cell = grid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));
		if (cell != currentCell)
		{
			currentCell = cell;
			return true;
		}
		return false;
	}

	void DoSelection()
	{
		grid.ClearPath();
		UpdateCurrentCell();
		if (currentCell)
		{
			HexUI.Instance.DisableAllUnitsUI();
			selectedUnit = currentCell.Unit;
			if (selectedUnit is Base @base)
			{
				MainBaseUI.Instance.ShowForBase(@base);
			}
			else if (selectedUnit is Squad squad && squad.squadType == SquadType.Player)
			{
				UIReferences.playerSquadUI.ShowForSquad(squad);
			}
		}
	}

	void DoPathfinding()
	{
		if (UpdateCurrentCell())
		{
			if (currentCell)
			{
				if (selectedUnit && (selectedUnit is Base || selectedUnit is Squad squad && squad.squadType == SquadType.Player) && selectedUnit.IsValidDestination(currentCell) && selectedUnit.Location != currentCell)
				{
					grid.FindPath(selectedUnit.Location, currentCell, selectedUnit);
				}
				else
				{
					grid.ClearPath();
				}
			}
		}
	}

	void DoMove()
	{
		if (grid.HasPath)
		{
			if (selectedUnit)
				selectedUnit.Travel(grid.GetPath());
			grid.ClearPath();
		}
	}
}
