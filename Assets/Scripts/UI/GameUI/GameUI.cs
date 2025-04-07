using System.Reflection;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameUI : MonoBehaviour
{
	public HexGrid grid;
	HexCell currentCell;
	public Unit selectedUnit;
	public HexUI hexUI;
	public GameObject panel;

	public RectTransform currentTurnLable;
	public int CurrentTurn
	{
		set
		{
			selectedUnit = null;
			currentCell = null;
			currentTurnLable.GetComponent<TMP_Text>().text = value.ToString();
		}
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
				if (squad.squadType == Character.CharacterType.Player)
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

	public void ToggleEditMode(bool toggle)
	{
		grid.ShowUI(toggle);
		grid.ClearPath();
		panel.SetActive(toggle);

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
			hexUI.DisableAllUnitsUI();
			selectedUnit = currentCell.Unit;
			if (selectedUnit is Base @base)
			{
				hexUI.mainBaseUI.ShowForBase(@base);
			}
			else if (selectedUnit is Squad squad && squad.squadType == Character.CharacterType.Player)
			{
				hexUI.playerSquadUI.ShowForSquad(squad);
			}
		}
	}

	void DoPathfinding()
	{
		if (UpdateCurrentCell())
		{
			if (currentCell && selectedUnit.IsValidDestination(currentCell, false))
			{
				if (selectedUnit is Base selectedBase)
				{
					grid.FindPath(selectedBase.Location, currentCell, selectedBase);
				}
				else if (selectedUnit is Squad selectedSquad &&
				(currentCell.Unit ? currentCell.Unit is Squad currentCellSquad && currentCellSquad.squadType != Character.CharacterType.Player && currentCell.Unit is not Base : true))
				{
					grid.FindPath(selectedSquad.Location, currentCell, selectedSquad);
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
