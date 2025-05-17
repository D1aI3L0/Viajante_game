#if !MIRROR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnemyAIHelper
{
    public static BattleEntity ChooseBestTarget(EnemyBattleCharacter enemy)
    {
        Dictionary<BattleEntity, float> scores = new Dictionary<BattleEntity, float>();

        foreach (var target in BattleManager.Instance.GetAllAllies())
        {
            float score = 0;

            // Весовые коэффициенты
            float distanceWeight = 0.4f;
            float healthWeight = 0.3f;
            float threatWeight = 0.3f;

            score += (1 / GetDistance(enemy, target)) * distanceWeight;
            score += (1 - target.CurrentHP / (float)target.maxHP) * healthWeight;
            score += CalculateThreatLevel(target) * threatWeight;

            scores[target] = score;
        }

        return scores.OrderByDescending(x => x.Value).FirstOrDefault().Key;
    }

    public static BattleCell ChooseBestMovementTarget(EnemyBattleCharacter enemy)
    {
        var bestTarget = ChooseBestTarget(enemy);

        if (bestTarget != null)
        {
            // Ищем ближайшую к цели свободную клетку
            return FindNearestWalkableCell(bestTarget.currentCell, enemy);
        }
        return null;
    }

    private static BattleCell FindNearestWalkableCell(BattleCell targetCell, BattleEntity mover)
    {
        for (int radius = 1; radius <= 3; radius++)
        {
            foreach (BattleCell cell in BattleMapManager.Instance.GetCellsInRadius(targetCell, radius))
            {
                if (cell.IsWalkable && cell.occupant == null)
                    return cell;
            }
        }
        return null;
    }

    public static float GetDistance(BattleEntity a, BattleEntity b)
    {
        return Vector3.Distance(a.transform.position, b.transform.position);
    }

    // Метод для оценки уровня угрозы цели
    public static float CalculateThreatLevel(BattleEntity target)
    {
        // Пример: угроза зависит от здоровья и уровня атаки
        float healthFactor = 1f - (target.CurrentHP / (float)target.maxHP);
        float attackFactor = (target is AllyBattleCharacter ally) ? ally.CurrentATK / 100f : 0f;
        return healthFactor + attackFactor;
    }
}
#endif