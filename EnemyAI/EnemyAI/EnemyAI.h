#pragma once

#ifdef ENEMYAI_EXPORTS
#define ENEMYAI_API __declspec(dllexport)
#else
#define ENEMYAI_API __declspec(dllimport)
#endif

#include <vector>

struct CellData {
    int x, z;
    bool isWalkable;
    bool hasObstacle;
};

struct EntityData {
    int id;
    int x, z;
    bool isEnemy;
    int hp;
    int skillIDs[5]; // Максимум 5 навыков
};

struct AIAction {
    int actionType; // 0 - движение, 1 - навык
    int targetX, targetZ;
    int skillID;
};

extern "C" {
    ENEMYAI_API AIAction CalculateEnemyAction(
        const CellData* map,
        int mapSize,
        const EntityData* entities,
        int entityCount,
        int currentEnemyID
    );
}