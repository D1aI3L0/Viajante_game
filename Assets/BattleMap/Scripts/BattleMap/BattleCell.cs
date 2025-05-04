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
    // ------------------------- Public Fields -------------------------

    public int xСoordinate, zСoordinate; // Координаты ячейки

    // ------------------------- Private Fields -------------------------

    // Префаб клетки (используем для установки визуальной модели)
    private GameObject prefabBattleCell;

    // Состояние клетки
    [SerializeField] private CellState state = CellState.Free;

    // Поля, связанные с препятствием и занятым персонажем
    [SerializeField] private GameObject _obstacleObject;
    [SerializeField] private BattleEntity _occupant;

    // Соседи клетки (индексируется согласно направлению, например, через HexDirection)
    [SerializeField] private BattleCell[] neighbors;

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
    public BattleEntity occupant
    {
        get => _occupant;
        set
        {
            _occupant = value;
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

    // ------------------------- Methods -------------------------

    #region Occupant Management

    /// <summary>
    /// Очищает клетку – удаляет ссылку на занятого персонажа и обновляет состояние.
    /// </summary>
    public void ClearOccupant()
    {
        occupant = null;
        UpdateCellState();
    }

    /// <summary>
    /// Устанавливает нового Occupant (персонажа) в клетку и обновляет состояние.
    /// </summary>
    public void SetOccupant(BattleEntity newOccupant)
    {
        occupant = newOccupant;
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
        else if (_occupant != null)
        {
            State = _occupant.IsEnemy ? CellState.Enemy : CellState.Ally;
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

    // public void OnCellClicked()
    // {
    //     Debug.Log("Клетка " + name + " была нажата.");
    //     // Вы можете напрямую вызвать нужный метод, например:
    //     // PlayerTurnController.Instance.OnCellClicked(this);
    // }
}
