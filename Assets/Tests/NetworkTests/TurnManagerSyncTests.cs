#if MIRROR
using NUnit.Framework;
using UnityEngine;
using Mirror;
using System.Collections;
using System.Threading.Tasks;

namespace NetworkTests
{
    [TestFixture]
    public class TurnManagerSyncTests
    {
        private GameObject turnManagerGO;
        private TurnManagerMP turnManager;

        [SetUp]
        public void Setup()
        {
            if (SyncTestsSetup.networkManager == null)
                SyncTestsSetup.Setup();

            turnManagerGO = new GameObject("TurnManager");
            turnManagerGO.SetActive(false);
            turnManagerGO.AddComponent<NetworkIdentity>();

            turnManager = turnManagerGO.AddComponent<TurnManagerMP>();
            turnManager.skipAutoInitialization = true;

            turnManagerGO.SetActive(true);

            turnManager.StatrTest();

            if (NetworkServer.active)
            {
                NetworkServer.Spawn(turnManagerGO);
            }

            Task.Delay(1000).Wait();
        }

        [TearDown]
        public void TearDown()
        {
            if (SyncTestsSetup.networkManager != null)
                SyncTestsSetup.TearDown();
            Object.DestroyImmediate(turnManagerGO);
        }

        [Test]
        public async Task TestSyncParticipants()
        {
            GameObject unitGO = new GameObject("Unit");
            BattleEntityMP unit = unitGO.AddComponent<BattleEntityMP>();
            unitGO.AddComponent<NetworkIdentity>();

            turnManager.RegisterParticipant(unit);
            await Task.Delay(500);

            Assert.IsTrue(turnManager.SyncParticipants.Contains(unit.netId), "Юнит должен присутствовать в SyncParticipants на сервере.");

            await Task.Delay(500);
            Assert.IsTrue(turnManager.GetParticipants().Contains(unit.netId), "SyncParticipants должен синхронизироваться с клиентом.");
        }

        [Test]
        public async Task TestCurrentActiveUnitSync()
        {
            GameObject unitGO = new GameObject("Unit");
            BattleEntityMP unit = unitGO.AddComponent<BattleEntityMP>();
            unitGO.AddComponent<NetworkIdentity>();

            turnManager.RegisterParticipant(unit);
            await Task.Delay(500);

            turnManager.currentActiveUnit = unit;
            await Task.Delay(500);

            Assert.AreEqual(unit, turnManager.currentActiveUnit, "Текущий активный юнит должен синхронизироваться между сервером и клиентами.");
        }

        [Test]
        public async Task TestEndTurnSync()
        {
            GameObject unitGO = new GameObject("Unit");
            BattleEntityMP unit = unitGO.AddComponent<BattleEntityMP>();
            unitGO.AddComponent<NetworkIdentity>();

            turnManager.RegisterParticipant(unit);
            await Task.Delay(500);

            turnManager.CmdEndTurn();
            await Task.Delay(500);

            Assert.AreEqual(0, unit.turnGauge, "Шкала хода юнита должна сбрасываться после завершения хода.");

            await Task.Delay(500);
            Assert.AreEqual(0, TurnManagerMP.Instance.GetParticipantById(turnManager.GetParticipants()[0]).turnGauge, "Сброс шкалы хода должен синхронизироваться между сервером и клиентами.");
        }

        [Test]
        public async Task TestRegisterAndUnregisterParticipantSync()
        {
            GameObject unitGO = new GameObject("Unit");
            BattleEntityMP unit = unitGO.AddComponent<BattleEntityMP>();
            unitGO.AddComponent<NetworkIdentity>();

            turnManager.RegisterParticipant(unit);
            await Task.Delay(500);

            Assert.IsTrue(turnManager.SyncParticipants.Contains(unit.netId), "Юнит должен присутствовать в SyncParticipants после регистрации.");

            turnManager.UnregisterParticipant(unit);
            await Task.Delay(500);

            Assert.IsFalse(turnManager.SyncParticipants.Contains(unit.netId), "Юнит должен быть удален из SyncParticipants после удаления.");

            await Task.Delay(500);
            Assert.IsFalse(turnManager.GetParticipants().Contains(unit.netId), "SyncParticipants должен синхронизироваться при удалении юнита.");
        }
    }
}
#endif