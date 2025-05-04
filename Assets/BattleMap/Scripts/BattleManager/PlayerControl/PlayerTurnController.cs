using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTurnController : MonoBehaviour
{
    public static PlayerTurnController Instance { get; private set; }

    // Текущий контролируемый персонаж (сейчас предполагается, что это AllyBattleCharacter)
    private AllyBattleCharacter currentPlayerUnit;

    // Компонент LineRenderer для визуального отображения маршрута
    public LineRenderer lineRenderer;

    // Рассчитанный маршрут (список клеток, по которым пойдёт юнит)
    private List<BattleCell> currentPath;

    private void Awake()
    {
        // Устанавливаем синглтон
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --------------------- Методы начала/окончания хода ---------------------

    public void StartTurn(BattleEntity playerUnit)
    {
        currentPlayerUnit = playerUnit as AllyBattleCharacter;
        if (currentPlayerUnit == null)
        {
            Debug.LogError("Переданный юнит не является AllyBattleCharacter.");
            return;
        }
        Debug.Log("Ход игрока: " + currentPlayerUnit.name);
        ClearPathDisplay();
        currentPath = null;
        // Здесь можно включить UI и активировать обработку кликов
    }

    public void EndTurn()
    {
        ClearPathDisplay();
        currentPlayerUnit = null;
        BattleManager.Instance.OnTurnComplete();
    }

    // --------------------- Обработка построения маршрута ---------------------

    /// <summary>
    /// Метод, вызываемый при клике по клетке.
    /// </summary>
    /// <param name="targetCell">Клетка, по которой кликнули.</param>
    public void OnCellClicked(BattleCell targetCell)
    {
        Debug.Log("Start cell: " + (currentPlayerUnit.currentCell != null ? currentPlayerUnit.currentCell.name : "null") +
          "; Target cell: " + targetCell.name);


        if (currentPlayerUnit == null)
        {
            Debug.LogError("Текущий игрок не установлен.");
            return;
        }

        // Вычисляем маршрут от текущей клетки до выбранной
        BattleCell startCell = currentPlayerUnit.currentCell;
        currentPath = PathFinder.FindPath(startCell, targetCell);
        if (currentPath != null)
        {
            int cost = currentPath.Count * currentPlayerUnit.SPmovecost;
            if (cost > currentPlayerUnit.currentSP)
            {
                Debug.Log("Недостаточно SP для заданного маршрута.");
                ClearPathDisplay();
                return;
            }
            UpdatePathDisplay(currentPath);
        }
        else
        {
            Debug.Log("Путь не найден.");
            ClearPathDisplay();
        }
    }

    private void UpdatePathDisplay(List<BattleCell> path)
    {
        if (lineRenderer == null)
            return;

        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, path[i].transform.position + Vector3.up * 0.1f);
        }
    }

    private void ClearPathDisplay()
    {
        if (lineRenderer != null)
            lineRenderer.positionCount = 0;
    }

    // --------------------- Перемещение по маршруту ---------------------

    public void ConfirmMove()
    {
        if (currentPath != null && currentPath.Count > 0)
        {
            StartCoroutine(MoveAlongPath(currentPath));
        }
    }

    private IEnumerator MoveAlongPath(List<BattleCell> path)
    {
        foreach (BattleCell nextCell in path)
        {
            currentPlayerUnit.currentSP -= currentPlayerUnit.SPmovecost;

            BattleCell previousCell = currentPlayerUnit.currentCell;
            if (previousCell != null)
            {
                previousCell.ClearOccupant();
            }

            nextCell.SetOccupant(currentPlayerUnit);

            Vector3 startPos = currentPlayerUnit.transform.position;
            Vector3 endPos = nextCell.transform.position;
            float elapsed = 0f;
            float duration = 0.25f;
            while (elapsed < duration)
            {
                currentPlayerUnit.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            currentPlayerUnit.transform.position = endPos;

            currentPlayerUnit.currentCell = nextCell;
            yield return new WaitForSeconds(0.1f);
        }
        ClearPathDisplay();
        currentPath = null;

        EndTurn();
    }

    // --------------------- Корректировка маршрута ---------------------

    public void AddWaypoint(BattleCell keyCell)
    {
        if (currentPath == null || currentPath.Count == 0)
            return;

        BattleCell start = currentPlayerUnit.currentCell;
        List<BattleCell> firstSegment = PathFinder.FindPath(start, keyCell);
        List<BattleCell> secondSegment = PathFinder.FindPath(keyCell, currentPath[currentPath.Count - 1]);

        if (firstSegment != null && secondSegment != null)
        {
            List<BattleCell> newPath = new List<BattleCell>(firstSegment);
            newPath.AddRange(secondSegment.GetRange(1, secondSegment.Count - 1));

            int cost = newPath.Count * currentPlayerUnit.SPmovecost;
            if (cost > currentPlayerUnit.currentSP)
            {
                Debug.Log("Недостаточно SP для маршрута с ключевой точкой.");
                return;
            }
            currentPath = newPath;
            UpdatePathDisplay(currentPath);
        }
    }
}
