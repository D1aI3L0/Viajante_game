using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EnemyTurnController : MonoBehaviour
{
    public virtual void StartTurn(EnemyBattleCharacterSP enemy)
    {
        StartCoroutine(ProcessAITurn(enemy));
    }

    private IEnumerator ProcessAITurn(EnemyBattleCharacterSP enemy)
    {
        var mapData = PrepareMapData();
        var entitiesData = PrepareEntitiesData();
        int enemyID = enemy.GetInstanceID();

        EnemyAIBridge.AIAction action = CalculateAIAction(mapData, entitiesData, enemyID);

        yield return ExecuteAction(enemy, action);

        BattleManagerSP.Instance.OnTurnComplete();
    }

    private EnemyAIBridge.AIAction CalculateAIAction(
    EnemyAIBridge.CellData[] mapData,
    EnemyAIBridge.EntityData[] entitiesData,
    int enemyID)
    {
        IntPtr mapPtr = SerializeToPointer(mapData);
        IntPtr entitiesPtr = SerializeToPointer(entitiesData);

        var result = EnemyAIBridge.CalculateEnemyAction(
            mapPtr,
            mapData.Length,
            entitiesPtr,
            entitiesData.Length,
            enemyID
        );

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

    private EnemyAIBridge.CellData[] PrepareMapData()
    {
        BattleCell[] allCells = BattleMapManagerSP.Instance.GetAllCells();
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
        List<BattleEntitySP> allEntities = BattleManagerSP.Instance.GetAllEntities();
        EnemyAIBridge.EntityData[] entitiesData = new EnemyAIBridge.EntityData[allEntities.Count];

        for (int i = 0; i < allEntities.Count; i++)
        {
            entitiesData[i] = new EnemyAIBridge.EntityData
            {
                id = allEntities[i].GetInstanceID(),
                x = allEntities[i].CurrentCell.xСoordinate,
                z = allEntities[i].CurrentCell.zСoordinate,
                isEnemy = allEntities[i].IsEnemy,
                hp = allEntities[i].CurrentHP,
                skillIDs = allEntities[i].GetSkillIDs()
            };
        }
        return entitiesData;
    }

    private IEnumerator ExecuteAction(EnemyBattleCharacterSP enemy, EnemyAIBridge.AIAction action)
    {
        switch (action.actionType)
        {
            case 0: // Движение
                BattleCell targetCell = BattleMapManagerSP.Instance.GetCell(action.targetX, action.targetZ);
                List<BattleCell> path = PathFinder.FindPath(enemy.CurrentCell, targetCell);

                if (path != null && path.Count > 0)
                {
                    yield return StartCoroutine(MoveAlongPath(enemy, path));
                }
                break;
            case 1: // Навык
                SkillAsset skill = enemy.GetSkill(action.skillID);
                SkillHandlerSP.Instance.ExecuteSkill(skill, enemy, GetTarget(skill));
                break;
        }
    }

    private BattleEntitySP GetTarget(SkillAsset skill)
    {
        if (skill.skillType == SkillType.Attack)
        {
            return FindClosestAlly();
        }
        return null;
    }

    private BattleEntitySP FindClosestAlly()
    {
        List<BattleEntitySP> allies = BattleManagerSP.Instance.Allies;
        BattleEntitySP closest = null;
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
    }

    private IEnumerator MoveAlongPath(EnemyBattleCharacterSP enemy, List<BattleCell> path)
    {
        foreach (var cell in path)
        {
            if (cell == null || !cell.IsWalkable) continue;

            Vector3 startPos = enemy.transform.position;
            float duration = 0.5f;
            float elapsed = 0;

            while (elapsed < duration)
            {
                enemy.transform.position = Vector3.Lerp(startPos, cell.transform.position, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            enemy.CurrentCell.ClearOccupantSP();
            cell.SetOccupant(enemy);
            enemy.CurrentCell = cell;
        }
    }
}