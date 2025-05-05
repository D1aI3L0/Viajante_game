using System.Collections;
using UnityEngine;

public class EnemyTurnController : MonoBehaviour
{
    /// <summary>
    /// Запускается, когда наступает ход врага.
    /// </summary>
    public void StartTurn(BattleEntity enemyUnit)
    {
        Debug.Log("Ход врага: " + enemyUnit.name);
        // Здесь можно реализовать логику AI.
        // Для простоты – запустим корутину, которая через 1 секунду завершит ход.
        StartCoroutine(SimulateEnemyTurn(enemyUnit));
    }

    private IEnumerator SimulateEnemyTurn(BattleEntity enemyUnit)
    {
        // Имитируем 1-секундное ожидание действий врага.
        yield return new WaitForSeconds(1f);
        Debug.Log("Враг (" + enemyUnit.name + ") совершил действие и завершил ход.");
        BattleManager.Instance.OnTurnComplete();
    }
}
