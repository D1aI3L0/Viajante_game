#include "pch.h"
#include "EnemyAI.h"
#include <cmath>
#include <limits>

#define NOMINMAX

ENEMYAI_API AIAction CalculateEnemyAction(
    const CellData* map,
    int mapSize,
    const EntityData* entities,
    int entityCount,
    int currentEnemyID
) {
    AIAction action = { 0 };
    const EntityData* currentEnemy = nullptr;

    // ����� �������� ����� �� ID
    for (int i = 0; i < entityCount; ++i) {
        if (entities[i].id == currentEnemyID) {
            currentEnemy = &entities[i];
            break;
        }
    }

    if (!currentEnemy) return action;

    // ����� ���������� ��������
    const EntityData* closestAlly = nullptr;
    float minDistance = (std::numeric_limits<float>::max)();

    for (int i = 0; i < entityCount; ++i) {
        if (!entities[i].isEnemy) {
            float dx = entities[i].x - currentEnemy->x;
            float dz = entities[i].z - currentEnemy->z;
            float distance = sqrt(dx * dx + dz * dz);

            if (distance < minDistance) {
                minDistance = distance;
                closestAlly = &entities[i];
            }
        }
    }

    // ����������� ������: �������� � ����
    if (closestAlly) {
        action.actionType = 0; // ��������
        action.targetX = closestAlly->x;
        action.targetZ = closestAlly->z;
    }

    return action;
}