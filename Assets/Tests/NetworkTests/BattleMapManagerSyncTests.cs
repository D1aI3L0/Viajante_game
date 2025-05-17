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
        private BattleMapManager serverBattleMapManager;
        private BattleMapManager clientBattleMapManager;

        // Dummy‑объекты для зависимостей
        private BattleConfig dummyBattleConfig;
        private GameObject dummyHexPrefabGO;
        private GameObject dummyBattleCellPrefabGO;
        private GameObject dummyObstaclePrefabGO;

        [SetUp]
        public void Setup()
        {
            if (SyncTestsSetup.networkManager == null)
                SyncTestsSetup.Setup();

            // Создаем GameObject для BattleMapManager и добавляем компонент
            battleMapManagerGO = new GameObject("BattleMapManager");
            battleMapManagerGO.SetActive(false);
            battleMapManagerGO.AddComponent<NetworkIdentity>();

            // Добавляем компонент; так как объект неактивен, Awake не будет вызван сразу
            serverBattleMapManager = battleMapManagerGO.AddComponent<BattleMapManager>();
            // Настраиваем флаг пропуска автоматической инициализации
            serverBattleMapManager.skipAutoInitialization = true;

            // Создаем dummy‑объекты для зависимостей:
            dummyBattleConfig = ScriptableObject.CreateInstance<BattleConfig>();
            // Задаем размер карты 5x5
            dummyBattleConfig.battleMapSize = new Vector2Int(5, 5);
            // Устанавливаем процент препятствий (например, 20%)
            dummyBattleConfig.obstaclePercent = 0.2f;

            // Dummy‑префабы: минимальные объекты, необходимые для инициализации
            dummyHexPrefabGO = new GameObject("DummyHexPrefab", typeof(BattleCell));
            dummyBattleCellPrefabGO = new GameObject("DummyBattleCellPrefab");
            dummyObstaclePrefabGO = new GameObject("DummyObstaclePrefab");

            // Назначаем зависимости в BattleMapManager
            serverBattleMapManager.battleConfig = dummyBattleConfig;
            serverBattleMapManager.hexPrefab = dummyHexPrefabGO.GetComponent<BattleCell>();
            serverBattleMapManager.prefabBattleCells = new GameObject[] { dummyBattleCellPrefabGO };
            serverBattleMapManager.obstaclePrefab = dummyObstaclePrefabGO;

            // Инициируем процесс создания карты: вызываем Awake() и Start()
            battleMapManagerGO.SetActive(true);

            serverBattleMapManager.TestAwake();

            if (NetworkServer.active)
            {
                NetworkServer.Spawn(battleMapManagerGO);
            }

            // Так как в режиме host сервер и клиент используют один экземпляр,
            // для проверки синхронизации используем BattleMapManager.Instance как клиентскую ссылку
            clientBattleMapManager = BattleMapManager.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            if (SyncTestsSetup.networkManager != null)
                SyncTestsSetup.TearDown();
            // Останавливаем сеть и уничтожаем созданные объекты
            Object.DestroyImmediate(battleMapManagerGO);
            Object.DestroyImmediate(dummyHexPrefabGO);
            Object.DestroyImmediate(dummyBattleCellPrefabGO);
            Object.DestroyImmediate(dummyObstaclePrefabGO);
        }

        [Test]
        public async Task TestMapSynchronization()
        {
            // Вызываем серверный метод генерации карты, который собирает данные и вызывает RpcSyncMap
            serverBattleMapManager.ServerGenerateMap();

            // Ожидаем асинхронно, чтобы RPC успели распространиться на клиентскую сторону
            await Task.Delay(1000);

            // Получаем список ячеек на стороне клиента
            BattleCell[] clientCells = clientBattleMapManager.GetAllCells();
            Assert.IsNotNull(clientCells, "Список ячеек не должен быть null после синхронизации.");
            Assert.IsTrue(clientCells.Length > 0, "Должны быть созданы ячейки на клиенте.");

            // Проверяем, что хотя бы одна ячейка содержит препятствие
            bool foundObstacle = clientCells.Any(cell => cell.ObstacleObject != null);
            Assert.IsTrue(foundObstacle, "После синхронизации хотя бы одна ячейка должна иметь препятствие.");
        }
    }
}
#endif