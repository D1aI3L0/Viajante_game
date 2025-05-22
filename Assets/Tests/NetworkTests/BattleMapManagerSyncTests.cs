#if MIRROR
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Mirror;
using NUnit.Framework.Internal;


namespace NetworkTests
{
    [TestFixture]
    public class BattleMapManagerSyncTests
    {
        private GameObject battleMapManagerGO;
        private BattleMapManagerMP serverBattleMapManager;
        private BattleMapManagerMP clientBattleMapManager;

        private BattleConfig dummyBattleConfig;
        private GameObject dummyHexPrefabGO;
        private GameObject dummyBattleCellPrefabGO;
        private GameObject dummyObstaclePrefabGO;

        [SetUp]
        public void Setup()
        {
            if (SyncTestsSetup.networkManager == null)
                SyncTestsSetup.Setup();

            battleMapManagerGO = new GameObject("BattleMapManager");
            battleMapManagerGO.SetActive(false);
            battleMapManagerGO.AddComponent<NetworkIdentity>();

            serverBattleMapManager = battleMapManagerGO.AddComponent<BattleMapManagerMP>();
            serverBattleMapManager.skipAutoInitialization = true;

            dummyBattleConfig = ScriptableObject.CreateInstance<BattleConfig>();
            dummyBattleConfig.battleMapSize = new Vector2Int(5, 5);
            dummyBattleConfig.obstaclePercent = 0.2f;

            dummyHexPrefabGO = new GameObject("DummyHexPrefab", typeof(BattleCell));
            dummyBattleCellPrefabGO = new GameObject("DummyBattleCellPrefab");
            dummyObstaclePrefabGO = new GameObject("DummyObstaclePrefab");

            serverBattleMapManager.battleConfig = dummyBattleConfig;
            serverBattleMapManager.hexPrefab = dummyHexPrefabGO.GetComponent<BattleCell>();
            serverBattleMapManager.prefabBattleCells = new GameObject[] { dummyBattleCellPrefabGO };
            serverBattleMapManager.obstaclePrefab = dummyObstaclePrefabGO;

            battleMapManagerGO.SetActive(true);

            serverBattleMapManager.TestAwake();

            if (NetworkServer.active)
            {
                NetworkServer.Spawn(battleMapManagerGO);
            }

            clientBattleMapManager = BattleMapManagerMP.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            if (SyncTestsSetup.networkManager != null)
                SyncTestsSetup.TearDown();
            Object.DestroyImmediate(battleMapManagerGO);
            Object.DestroyImmediate(dummyHexPrefabGO);
            Object.DestroyImmediate(dummyBattleCellPrefabGO);
            Object.DestroyImmediate(dummyObstaclePrefabGO);
        }

        [Test]
        public async Task TestMapSynchronization()
        {
            serverBattleMapManager.ServerGenerateMap();

            await Task.Delay(1000);

            BattleCell[] clientCells = clientBattleMapManager.GetAllCells();
            Assert.IsNotNull(clientCells, "Список ячеек не должен быть null после синхронизации.");
            Assert.IsTrue(clientCells.Length > 0, "Должны быть созданы ячейки на клиенте.");

            bool foundObstacle = clientCells.Any(cell => cell.ObstacleObject != null);
            Assert.IsTrue(foundObstacle, "После синхронизации хотя бы одна ячейка должна иметь препятствие.");
        }
    }
}
#endif