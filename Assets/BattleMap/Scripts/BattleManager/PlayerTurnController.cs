using UnityEngine;

public class PlayerTurnController : MonoBehaviour
{
    private BattleEntity currentPlayerUnit;

    /// <summary>
    /// Запускается, когда наступает ход игрока.
    /// </summary>
    public void StartTurn(BattleEntity playerUnit)
    {
        currentPlayerUnit = playerUnit;
        Debug.Log("Ход игрока: " + playerUnit.name);
        
        // Здесь можно включить UI для управления юнитом,
        // дать игроку возможность совершить действие и т.д.
    }

    /// <summary>
    /// Метод, который вызывается, когда игрок завершает свой ход.
    /// Например, при нажатии кнопки «Завершить ход».
    /// </summary>
    public void EndTurn()
    {
        // Отключаем UI и другие элементы управления, если нужно.
        // Уведомляем BattleManager, что ход завершён.
        BattleManager.Instance.OnTurnComplete();
        currentPlayerUnit = null;
    }
}
