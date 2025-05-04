using UnityEngine;

public class CellClickForwarder : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Получаем компонент BattleCell у родительского объекта
        BattleCell cell = GetComponentInParent<BattleCell>();
        if (cell == null)
        {
            Debug.LogWarning("Не найден компонент BattleCell у родительского объекта.");
            return;
        }
        
        // Проверяем, зажата ли клавиша Shift (можно расширить для права или обеих клавиш)
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Вызываем метод добавления промежуточной точки
            if (PlayerTurnController.Instance != null)
            {
                Debug.Log("Shift+Click по клетке: " + cell.name + " - добавляем ключевую точку.");
                PlayerTurnController.Instance.AddWaypoint(cell);
            }
            else
            {
                Debug.LogWarning("PlayerTurnController.Instance is null.");
            }
        }
        else
        {
            // Обычный клик – рассчитываем маршрут
            if (PlayerTurnController.Instance != null)
            {
                Debug.Log("Клик по клетке: " + cell.name);
                PlayerTurnController.Instance.OnCellClicked(cell);
            }
            else
            {
                Debug.LogWarning("PlayerTurnController.Instance is null.");
            }
        }
    }
}
