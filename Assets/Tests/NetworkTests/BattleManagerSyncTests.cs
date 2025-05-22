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
        private BattleManagerMP battleManager;

        [SetUp]
        public void Setup()
        {
            if (SyncTestsSetup.networkManager == null)
                SyncTestsSetup.Setup();

            if (battleManager != null)
                return;

            battleManagerGO = new GameObject("BattleManager");
            battleManagerGO.SetActive(false);
            battleManagerGO.AddComponent<NetworkIdentity>();

            battleManager = battleManagerGO.AddComponent<BattleManagerMP>();
            battleManager.skipAutoInitialization = true;

            battleManagerGO.SetActive(true);

            battleManager.StartTest();

            if (NetworkServer.active)
            {
                NetworkServer.Spawn(battleManagerGO);
            }

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
            GameObject allyGO = new GameObject("Ally");
            BattleEntityMP allyEntity = allyGO.AddComponent<BattleEntityMP>();
            allyGO.AddComponent<NetworkIdentity>();

            battleManager.RegisterAlly(allyEntity);
            await Task.Delay(500);

            Assert.IsTrue(battleManager.SyncAllies.Contains(allyEntity.netId), "Союзник должен появиться в SyncAllies на сервере.");

            await Task.Delay(500);
            Assert.IsTrue(battleManager.GetAllAllies().Contains(allyEntity.netId), "SyncAllies должен синхронизироваться с клиентом.");
        }

        [Test]
        public async Task TestSyncEnemiesList()
        {
            GameObject enemyGO = new GameObject("Enemy");
            BattleEntityMP enemyEntity = enemyGO.AddComponent<BattleEntityMP>();
            enemyGO.AddComponent<NetworkIdentity>();

            battleManager.RegisterEnemy(enemyEntity);
            await Task.Delay(500);

            Assert.IsTrue(battleManager.SyncEnemies.Contains(enemyEntity.netId), "Враг должен появиться в SyncEnemies на сервере.");

            await Task.Delay(500);
            Assert.IsTrue(battleManager.GetAllEnemies().Contains(enemyEntity.netId), "SyncEnemies должен синхронизироваться с клиентом.");
        }

        [Test]
        public async Task TestPlayerReadyCheck()
        {
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

            bool result = battleManager.ServerCheckPlayersReady();
            await Task.Delay(500);
            Assert.IsTrue(result == true, "Не все игроки готовы");
        }

        [Test]
        public async Task TestEntityDeathSync()
        {
            GameObject allyGO = new GameObject("Ally");
            BattleEntityMP allyEntity = allyGO.AddComponent<BattleEntityMP>();
            allyGO.AddComponent<NetworkIdentity>();

            battleManager.RegisterAlly(allyEntity);
            await Task.Delay(500);

            Assert.IsTrue(battleManager.SyncAllies.Contains(allyEntity.netId), "Союзник должен присутствовать в SyncAllies перед удалением.");

            battleManager.ServerOnEntityDeath(allyEntity);
            await Task.Delay(500);

            Assert.IsFalse(battleManager.SyncAllies.Contains(allyEntity.netId), "Юнит должен быть удален из SyncAllies на сервере.");

            await Task.Delay(500);
            Assert.IsFalse(battleManager.GetAllAllies().Contains(allyEntity.netId), "SyncAllies должен синхронизироваться с клиентом при удалении юнита.");
        }
    }
}
#endif