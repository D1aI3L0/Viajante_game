using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EnemyTurnController : MonoBehaviour
{
    public virtual void StartTurn(EnemyBattleCharacter enemy)
    {
        StartCoroutine(ProcessAITurn(enemy));
    }

    private IEnumerator ProcessAITurn(EnemyBattleCharacter enemy)
    {
        // Сбор данных
        var mapData = PrepareMapData();
        var entitiesData = PrepareEntitiesData();
        int enemyID = enemy.GetInstanceID();

        // Вызов C++ DLL
        EnemyAIBridge.AIAction action = CalculateAIAction(mapData, entitiesData, enemyID);

        // Выполнение действия
        yield return ExecuteAction(enemy, action);

        BattleManager.Instance.OnTurnComplete();
    }

    private EnemyAIBridge.AIAction CalculateAIAction(
    EnemyAIBridge.CellData[] mapData,
    EnemyAIBridge.EntityData[] entitiesData,
    int enemyID)
    {
        IntPtr mapPtr = SerializeToPointer(mapData);
        IntPtr entitiesPtr = SerializeToPointer(entitiesData);

        // Вызываем метод из DLL
        var result = EnemyAIBridge.CalculateEnemyAction(
            mapPtr,
            mapData.Length,
            entitiesPtr,
            entitiesData.Length,
            enemyID
        );

        // Освобождаем память
        Marshal.FreeHGlobal(mapPtr);
        Marshal.FreeHGlobal(entitiesPtr);

        return result;
    }

    private IntPtr SerializeToPointer<T>(T[] data)
    {
        int size = Marshal.SizeOf(typeof(T));
        IntPtr ptr = Marshal.AllocHGlobal(size * data.Length);

        for (int i = 0; i < data.Length; i++)
        {
            IntPtr elementPtr = new IntPtr(ptr.ToInt64() + i * size);
            Marshal.StructureToPtr(data[i], elementPtr, false);
        }

        return ptr;
    }

    // EnemyTurnController.cs
    private EnemyAIBridge.CellData[] PrepareMapData()
    {
        BattleCell[] allCells = BattleMapManager.Instance.GetAllCells();
        EnemyAIBridge.CellData[] mapData = new EnemyAIBridge.CellData[allCells.Length];

        for (int i = 0; i < allCells.Length; i++)
        {
            mapData[i] = new EnemyAIBridge.CellData
            {
                x = allCells[i].xСoordinate,
                z = allCells[i].zСoordinate,
                isWalkable = allCells[i].IsWalkable,
                hasObstacle = allCells[i].State == CellState.Obstacle
            };
        }
        return mapData;
    }

    private EnemyAIBridge.EntityData[] PrepareEntitiesData()
    {
        List<BattleEntity> allEntities = BattleManager.Instance.GetAllEntities();
        EnemyAIBridge.EntityData[] entitiesData = new EnemyAIBridge.EntityData[allEntities.Count];

        for (int i = 0; i < allEntities.Count; i++)
        {
            entitiesData[i] = new EnemyAIBridge.EntityData
            {
                id = allEntities[i].GetInstanceID(),
                x = allEntities[i].currentCell.xСoordinate,
                z = allEntities[i].currentCell.zСoordinate,
                isEnemy = allEntities[i].IsEnemy,
                hp = allEntities[i].CurrentHP,
                skillIDs = allEntities[i].GetSkillIDs()
            };
        }
        return entitiesData;
    }

    private IEnumerator ExecuteAction(EnemyBattleCharacter enemy, EnemyAIBridge.AIAction action)
    {
        switch (action.actionType)
        {
            case 0: // Движение
                BattleCell targetCell = BattleMapManager.Instance.GetCell(action.targetX, action.targetZ);
                List<BattleCell> path = PathFinder.FindPath(enemy.currentCell, targetCell);

                if (path != null && path.Count > 0)
                {
                    yield return StartCoroutine(MoveAlongPath(enemy, path));
                }
                break;
            case 1: // Навык
                SkillAsset skill = enemy.GetSkill(action.skillID);
                SkillHandler.Instance.ExecuteSkill(skill, enemy, GetTarget(skill));
                break;
        }
    }

    // EnemyTurnController.cs
    private BattleEntity GetTarget(SkillAsset skill)
    {
        // Пример логики выбора цели:
        if (skill.skillType == SkillType.Attack)
        {
            return FindClosestAlly();
        }
        return null;
    }
    private BattleEntity FindClosestAlly()
    {

#if !MIRROR
        List<BattleEntity> allies = BattleManager.Instance.Allies;
        BattleEntity closest = null;
        float minDistance = Mathf.Infinity;

        foreach (var ally in allies)
        {
            float distance = Vector3.Distance(transform.position, ally.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = ally;
            }
        }
        return closest;
#else
        return null;
#endif
    }

    private IEnumerator MoveAlongPath(EnemyBattleCharacter enemy, List<BattleCell> path)
    {
        foreach (var cell in path)
        {
            if (cell == null || !cell.IsWalkable) continue;

            // Анимация перемещения
            Vector3 startPos = enemy.transform.position;
            float duration = 0.5f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                enemy.transform.position = Vector3.Lerp(startPos, cell.transform.position, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            enemy.currentCell.ClearOccupant();
            cell.SetOccupant(enemy);
            enemy.currentCell = cell;
        }
    }
}