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
    public int xСoordinate, zСoordinate; // Координаты ячейки

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

    //====================================== Состояние ячейки и объекты на ней ===========================================
    [SerializeField] private CellState state = CellState.Free;
    
    // Закрытые поля для препятствия и персонажа
    [SerializeField] private GameObject _obstacleObject; // препятствия
    [SerializeField] private BattleCharacter _occupant; // пресонажи

    // Публичное свойство для препятствия, которое будет обновлять состояние
    public GameObject ObstacleObject
    {
        get => _obstacleObject;
        set
        {
            _obstacleObject = value;
            UpdateCellState();
        }
    }

    // Публичное свойство для персонажа (occupant)
    public BattleCharacter occupant
    {
        get => _occupant;
        set
        {
            _occupant = value;
            UpdateCellState();
        }
    }

    // Свойство, которое возвращает статус доступности ячейки
    public bool IsWalkable => state == CellState.Free;

    public CellState State
    {
        get => state;
        private set => state = value;  // Только внутри класса можно менять состояние
    }
    //===============================================================================================================

    // Метод обновления состояния ячейки на основе установленных полей
    private void UpdateCellState()
    {
        // Если установлено препятствие, состояние всегда Obstacle.
        if (_obstacleObject != null)
        {
            State = CellState.Obstacle;
        }
        // Если препятствия нет, но есть персонаж, определим состояние по типу персонажа.
        else if (_occupant != null)
        {
            // Если у BattleCharacter есть свойство, указывающее тип (например, ally/enemy),
            // можно использовать его для определения состояния.
            // Здесь для примера предполагается, что все занятые ячейки с персонажем – это союзники.
            State = CellState.Ally;

            // Если в будущем появится различие, можно сделать что-то вроде:
            // State = _occupant.IsEnemy ? CellState.Enemy : CellState.Ally;
        }
        else
        {
            State = CellState.Free;
        }
    }

    //================================================== Соседи ==========================================================
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
    //===============================================================================================================

    // ================================== Методы для отладки =================================
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
}
