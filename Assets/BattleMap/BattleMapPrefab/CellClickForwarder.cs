using UnityEngine;

public class CellClickForwarder : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Получаем ссылку на родительский объект BattleCell
        BattleCell cell = GetComponentInParent<BattleCell>();
        if (cell != null)
        {
            // Вызываем метод OnCellClicked у централизованного контроллера
            if (PlayerTurnController.Instance != null)
            {
                PlayerTurnController.Instance.OnCellClicked(cell);
            }
            else
            {
                Debug.LogWarning("PlayerTurnController.Instance is null. Проверьте настройку синглтона.");
            }
        }
    }
}
