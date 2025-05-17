#if MIRROR
using NUnit.Framework;
using UnityEngine;
using Mirror;
using System.Threading.Tasks;

namespace NetworkTests
{
    [TestFixture]
    public class BattleManagerSyncTests
    {
        private GameObject battleManagerGO;
        private BattleManager battleManager;

        [SetUp]
        public void Setup()
        {
            if (SyncTestsSetup.networkManager == null)
                SyncTestsSetup.Setup();

            if (battleManager != null)
                return;

            // Создание BattleManager
            battleManagerGO = new GameObject("BattleManager");
            battleManagerGO.SetActive(false);
            battleManagerGO.AddComponent<NetworkIdentity>();

            battleManager = battleManagerGO.AddComponent<BattleManager>();
            battleManager.skipAutoInitialization = true;

            battleManagerGO.SetActive(true);

            battleManager.StartTest();

            if (NetworkServer.active)
            {
                NetworkServer.Spawn(battleManagerGO);
            }

            // Ждем, чтобы сеть корректно стартовала
            Task.Delay(1000).Wait();
        }

        [TearDown]
        public void TearDown()
        {
            if (SyncTestsSetup.networkManager != null)
                SyncTestsSetup.TearDown();
            Object.DestroyImmediate(battleManagerGO);
        }

        [Test]
        public async Task TestSyncAlliesList()
        {
            // Создаем объект союзного юнита и добавляем его в список союзников через сервер
            GameObject allyGO = new GameObject("Ally");
            BattleEntity allyEntity = allyGO.AddComponent<BattleEntity>();
            allyGO.AddComponent<NetworkIdentity>();

            battleManager.RegisterAlly(allyEntity);
            await Task.Delay(500);

            // Проверяем, что юнит появился в SyncAllies на сервере
            Assert.IsTrue(battleManager.SyncAllies.Contains(allyEntity.netId), "Союзник должен появиться в SyncAllies на сервере.");

            // Проверяем, что клиент тоже получил обновленный список
            await Task.Delay(500);
            Assert.IsTrue(battleManager.GetAllAllies().Contains(allyEntity.netId), "SyncAllies должен синхронизироваться с клиентом.");
        }

        [Test]
        public async Task TestSyncEnemiesList()
        {
            // Создаем объект вражеского юнита и добавляем его в список врагов через сервер
            GameObject enemyGO = new GameObject("Enemy");
            BattleEntity enemyEntity = enemyGO.AddComponent<BattleEntity>();
            enemyGO.AddComponent<NetworkIdentity>();

            battleManager.RegisterEnemy(enemyEntity);
            await Task.Delay(500);

            // Проверяем, что юнит появился в SyncEnemies на сервере
            Assert.IsTrue(battleManager.SyncEnemies.Contains(enemyEntity.netId), "Враг должен появиться в SyncEnemies на сервере.");

            // Проверяем, что клиент тоже получил обновленный список
            await Task.Delay(500);
            Assert.IsTrue(battleManager.GetAllEnemies().Contains(enemyEntity.netId), "SyncEnemies должен синхронизироваться с клиентом.");
        }

        [Test]
        public async Task TestPlayerReadyCheck()
        {
            // Создаем объект игрока и добавляем его в список игроков
            GameObject playerGO = new GameObject("Player");
            NetworkPlayerController playerController = playerGO.AddComponent<NetworkPlayerController>();
            playerGO.AddComponent<NetworkIdentity>();

            playerController.isReady = true;

            if (NetworkServer.active)
            {
                NetworkServer.Spawn(playerGO);
            }

            battleManager.players.Add(playerController.netId);
            await Task.Delay(500);

            // Проверяем, что сервер корректно запускает бой, если все игроки готовы
            bool result = battleManager.ServerCheckPlayersReady();
            await Task.Delay(500);
            Assert.IsTrue(result == true, "Не все игроки готовы");
        }

        [Test]
        public async Task TestEntityDeathSync()
        {
            // Создаем объект союзного юнита и добавляем его в список союзников
            GameObject allyGO = new GameObject("Ally");
            BattleEntity allyEntity = allyGO.AddComponent<BattleEntity>();
            allyGO.AddComponent<NetworkIdentity>();

            battleManager.RegisterAlly(allyEntity);
            await Task.Delay(500);

            Assert.IsTrue(battleManager.SyncAllies.Contains(allyEntity.netId), "Союзник должен присутствовать в SyncAllies перед удалением.");

            // Симуляция удаления юнита (смерти)
            battleManager.ServerOnEntityDeath(allyEntity);
            await Task.Delay(500);

            // Проверяем, что юнит удален с сервера
            Assert.IsFalse(battleManager.SyncAllies.Contains(allyEntity.netId), "Юнит должен быть удален из SyncAllies на сервере.");

            // Проверяем, что клиент тоже получил обновленный список
            await Task.Delay(500);
            Assert.IsFalse(battleManager.GetAllAllies().Contains(allyEntity.netId), "SyncAllies должен синхронизироваться с клиентом при удалении юнита.");
        }
    }
}
#endif