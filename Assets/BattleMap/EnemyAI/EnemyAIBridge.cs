using System;
using System.Runtime.InteropServices;
using UnityEngine;

public static class EnemyAIBridge
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CellData
    {
        public int x, z;
        public bool isWalkable;
        public bool hasObstacle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EntityData
    {
        public int id;
        public int x, z;
        public bool isEnemy;
        public int hp;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public int[] skillIDs;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AIAction
    {
        public int actionType;
        public int targetX;
        public int targetZ;
        public int skillID;
    }

    [DllImport("EnemyAI")]
    public static extern AIAction CalculateEnemyAction(
        IntPtr mapData,
        int mapSize,
        IntPtr entitiesData,
        int entityCount,
        int currentEnemyID
    );
}