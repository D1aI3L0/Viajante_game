using System.Collections.Generic;
using UnityEngine;

public static class PathFinder
{
    // Функция для вычисления эвристики (расстояния) между двумя клетками.
    // Для шестиугольной сетки можно использовать расстояние по оси: 
    // в данном случае берем евклидово расстояние (можно заменить на hex-distance для точности).
    public static float Heuristic(BattleCell a, BattleCell b)
    {
        Vector3 diff = a.transform.position - b.transform.position;
        return diff.magnitude;
    }

    /// <summary>
    /// Находит путь от start до target с использованием алгоритма A*.
    /// Если путь найден, возвращает список клеток от start до target (включая их).
    /// Если путь не найден, возвращает null.
    /// </summary>
    public static List<BattleCell> FindPath(BattleCell start, BattleCell target)
    {
        if (start == null || target == null)
        {
            return null;
        }
        
        // Открытый список для клеток, которые еще необходимо проверить (приоритет по fCost = gCost + hCost).
        List<BattleCell> openSet = new List<BattleCell>();
        // Закрытый список – клетки, которые уже были оценены
        HashSet<BattleCell> closedSet = new HashSet<BattleCell>();

        // Для хранения стоимостей и ссылок на предыдущие клетки
        Dictionary<BattleCell, float> gCost = new Dictionary<BattleCell, float>();
        Dictionary<BattleCell, float> fCost = new Dictionary<BattleCell, float>();
        Dictionary<BattleCell, BattleCell> cameFrom = new Dictionary<BattleCell, BattleCell>();

        gCost[start] = 0;
        fCost[start] = Heuristic(start, target);
        openSet.Add(start);

        while (openSet.Count > 0)
        {
            // Выбираем клетку с минимальным fCost из openSet.
            BattleCell current = openSet[0];
            foreach (BattleCell cell in openSet)
            {
                if (fCost[cell] < fCost[current])
                    current = cell;
            }

            if (current == target)
            {
                // Путь найден, восстанавливаем путь
                return ReconstructPath(cameFrom, current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            // Перебираем соседей
            foreach (BattleCell neighbor in current.GetNeighbors())
            {
                if (neighbor == null)
                    continue;
                
                // Если соседняя клетка недоступна для движения, пропускаем её.
                if (!neighbor.IsWalkable)
                    continue;
                
                // Если клетка занята (occupant != null) и не является target, пропускаем её
                if ((neighbor.OccupantSP != null || neighbor.OccupantMP != null) && neighbor != target)
                    continue;

                if (closedSet.Contains(neighbor))
                    continue;

                // Допустим стоимость перемещения - 1
                float tentativeGCost = gCost[current] + 1;

                if (!openSet.Contains(neighbor) || tentativeGCost < gCost[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = tentativeGCost;
                    fCost[neighbor] = tentativeGCost + Heuristic(neighbor, target);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // Если openSet пуст и не найден путь, возвращаем null.
        return null;
    }

    /// <summary>
    /// Восстанавливает путь по словарю cameFrom.
    /// </summary>
    private static List<BattleCell> ReconstructPath(Dictionary<BattleCell, BattleCell> cameFrom, BattleCell current)
    {
        List<BattleCell> path = new List<BattleCell>();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Add(current); // Добавляем стартовую клетку
        path.Reverse();
        return path;
    }
}
