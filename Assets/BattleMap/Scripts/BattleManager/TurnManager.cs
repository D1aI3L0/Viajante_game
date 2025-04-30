using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    // Список участников боя, который можно заполнять через регистрацию (например, вручную или спавнером)
    public List<BattleEntity> participants = new List<BattleEntity>();
    
    // Собственная копия очереди ходов (если требуется сортировка по характеристикам)
    private List<BattleEntity> turnOrder = new List<BattleEntity>();
    private int currentTurnIndex = 0;
    
    // Можно добавить методы регистрации юнитов
    public void RegisterParticipant(BattleEntity entity)
    {
        if (!participants.Contains(entity))
        {
            participants.Add(entity);
        }
    }
    
    public void UnregisterParticipant(BattleEntity entity)
    {
        if (participants.Contains(entity))
            participants.Remove(entity);
    }
    
    // Метод сборки очереди ходов. Здесь можно сортировать по скорости или другим критериям.
    public void BuildTurnOrder()
    {
        turnOrder.Clear();
        turnOrder.AddRange(participants);
        
        // Пример сортировки по текущей скорости (от большей к меньшей)
        turnOrder.Sort((a, b) => b.currentSPD.CompareTo(a.currentSPD));
        
        currentTurnIndex = 0;
    }

    // Метод установки активного хода по индексу в очереди
    public void SetActiveTurn(int index)
    {
        // Сброс состояний для всех участников
        foreach (BattleEntity entity in participants)
        {
            entity.isActiveTurn = false;
        }
        
        if (turnOrder.Count == 0)
            return;
        
        // Обеспечиваем корректную индексацию в цикле
        currentTurnIndex = index % turnOrder.Count;
        turnOrder[currentTurnIndex].isActiveTurn = true;
        Debug.Log("Ход: " + turnOrder[currentTurnIndex].name);
        
        // Дополнительно можно оповестить UI об изменении активного участника
    }
    
    // Метод, вызываемый, когда ход текущего участника завершён
    public void EndCurrentTurn()
    {
        // Переключаем активный ход на следующего
        SetActiveTurn(currentTurnIndex + 1);
    }
    
    // Другие вспомогательные методы, если потребуется (например, пересчет очереди, если изменяются характеристики)
}
