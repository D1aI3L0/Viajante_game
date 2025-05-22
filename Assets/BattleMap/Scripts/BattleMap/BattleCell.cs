using System;
using UnityEngine;

public enum CellState
{
    Free,
    Ally,
    Enemy,
    Obstacle,
}

[Serializable]
public class BattleCell : MonoBehaviour
{
    // ------------------------- Public Fields -------------------------

    public int xСoordinate, zСoordinate; // Координаты ячейки

    // ------------------------- Private Fields -------------------------

    // Префаб клетки (используем для установки визуальной модели)
    private GameObject prefabBattleCell;

    // Состояние клетки
    [SerializeField] private CellState state = CellState.Free;

    // Поля, связанные с препятствием и занятым персонажем
    [SerializeField] private GameObject _obstacleObject;
    [SerializeField] private BattleEntitySP _occupantSP;
    [SerializeField] private BattleEntityMP _occupantMP;

    // Соседи клетки (индексируется согласно направлению, например, через HexDirection)
    [SerializeField] public BattleCell[] neighbors;

    // ------------------------- Properties -------------------------

    /// <summary>
    /// Свойство для установки префаба ячейки. При новой установке старый префаб уничтожается, и новый становится дочерним объектом.
    /// </summary>
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

    /// <summary>
    /// Свойство для получения/установки препятствия с обновлением состояния.
    /// </summary>
    public GameObject ObstacleObject
    {
        get => _obstacleObject;
        set
        {
            _obstacleObject = value;
            UpdateCellState();
        }
    }

    /// <summary>
    /// Свойство для получения/установки занятого персонажа (occupant) с обновлением состояния.
    /// </summary>
    public BattleEntitySP OccupantSP
    {
        get => _occupantSP;
        set
        {
            _occupantSP = value;
            UpdateCellState();
        }
    }

    public BattleEntityMP OccupantMP
    {
        get => _occupantMP;
        set
        {
            _occupantMP = value;
            UpdateCellState();
        }
    }

    /// <summary>
    /// Свойство, возвращающее, свободна ли клетка (т.е. состояние Free).
    /// </summary>
    public bool IsWalkable => state == CellState.Free;

    /// <summary>
    /// Свойство для доступа к состоянию клетки (только для чтения из вне).
    /// </summary>
    public CellState State
    {
        get => state;
        private set => state = value;
    }

    public void SetState(CellState newState)
    {
        State = newState;
    }


    public void SetObstacle(GameObject obstaclePrefab)
    {
        if (State != CellState.Obstacle)
        {
            State = CellState.Obstacle;
            if (obstaclePrefab != null && _obstacleObject == null)
            {
                _obstacleObject = Instantiate(obstaclePrefab, transform);
                _obstacleObject.transform.localPosition = Vector3.zero;
            }
        }
    }
    // ------------------------- Methods -------------------------

    #region Occupant Management

    /// <summary>
    /// Очищает клетку – удаляет ссылку на занятого персонажа и обновляет состояние.
    /// </summary>
    public void ClearOccupantSP()
    {
        OccupantSP = null;
        UpdateCellState();
    }

    /// <summary>
    /// Устанавливает нового Occupant (персонажа) в клетку и обновляет состояние.
    /// </summary>
    public void SetOccupant(BattleEntitySP newOccupant)
    {
        OccupantSP = newOccupant;
        UpdateCellState();
        if (newOccupant != null)
        {
            newOccupant.CurrentCell = this;
        }
    }

    public void SetOccupant(BattleEntityMP newOccupant)
    {
        OccupantMP = newOccupant;
        UpdateCellState();
        if (newOccupant != null)
        {
            newOccupant.currentCell = this;
        }
    }


    #endregion

    #region Cell State Management

    /// <summary>
    /// Обновляет состояние клетки (Free, Ally, Enemy, Obstacle) на основе установленных полей.
    /// </summary>
    private void UpdateCellState()
    {
        if (_obstacleObject != null)
        {
            State = CellState.Obstacle;
        }
        else if (_occupantSP != null)
        {
            State = _occupantSP.IsEnemy ? CellState.Enemy : CellState.Ally;
        }
        else if (_occupantMP != null)
        {
            State = _occupantMP.IsEnemy ? CellState.Enemy : CellState.Ally;
        }
        else
        {
            State = CellState.Free;
        }
    }

    #endregion

    #region Neighbors Management

    /// <summary>
    /// Возвращает соседа клетки по заданному направлению.
    /// </summary>
    public BattleCell GetNeighbor(HexDirection direction)
    {
        return neighbors[(int)direction];
    }

    /// <summary>
    /// Устанавливает для данной клетки соседа по направлению, а также связывает обратным образом.
    /// </summary>
    public void SetNeighbor(HexDirection direction, BattleCell cell)
    {
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }

    /// <summary>
    /// Возвращает массив соседних клеток.
    /// </summary>
    public BattleCell[] GetNeighbors()
    {
        return neighbors;
    }

    #endregion

    #region Debug & Gizmos

    /// <summary>
    /// Отрисовка Gizmos для отладки: отображает сферу в центре клетки и линии к соседям.
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
        if (neighbors != null)
        {
            foreach (var neighbor in neighbors)
            {
                if (neighbor != null)
                    Gizmos.DrawLine(transform.position, neighbor.transform.position);
            }
        }
    }

    #endregion
}