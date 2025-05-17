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
        private TurnManager turnManager;

        [SetUp]
        public void Setup()
        {
            if (SyncTestsSetup.networkManager == null)
                SyncTestsSetup.Setup();

            // Создание TurnManager
            turnManagerGO = new GameObject("TurnManager");
            turnManagerGO.SetActive(false);
            turnManagerGO.AddComponent<NetworkIdentity>();

            turnManager = turnManagerGO.AddComponent<TurnManager>();
            turnManager.skipAutoInitialization = true;

            turnManagerGO.SetActive(true);

            turnManager.StatrTest();

            if (NetworkServer.active)
            {
                NetworkServer.Spawn(turnManagerGO);
            }

            // Ожидание, чтобы сеть инициализировалась
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
            // Создаем тестовый юнит и добавляем его в список участников через сервер
            GameObject unitGO = new GameObject("Unit");
            BattleEntity unit = unitGO.AddComponent<BattleEntity>();
            unitGO.AddComponent<NetworkIdentity>();

            turnManager.RegisterParticipant(unit);
            await Task.Delay(500);

            // Проверяем, что юнит появился в SyncParticipants на сервере
            Assert.IsTrue(turnManager.SyncParticipants.Contains(unit.netId), "Юнит должен присутствовать в SyncParticipants на сервере.");

            // Проверяем синхронизацию списка участников на клиенте
            await Task.Delay(500);
            Assert.IsTrue(turnManager.GetParticipants().Contains(unit.netId), "SyncParticipants должен синхронизироваться с клиентом.");
        }

        [Test]
        public async Task TestCurrentActiveUnitSync()
        {
            // Создаем тестовый юнит и добавляем его в список участников
            GameObject unitGO = new GameObject("Unit");
            BattleEntity unit = unitGO.AddComponent<BattleEntity>();
            unitGO.AddComponent<NetworkIdentity>();

            turnManager.RegisterParticipant(unit);
            await Task.Delay(500);

            // Имитация выбора юнита как активного
            turnManager.currentActiveUnit = unit;
            await Task.Delay(500);

            // Проверяем, что SyncVar корректно обновляется на клиенте
            Assert.AreEqual(unit, turnManager.currentActiveUnit, "Текущий активный юнит должен синхронизироваться между сервером и клиентами.");
        }

        [Test]
        public async Task TestEndTurnSync()
        {
            // Создаем юнит и добавляем его в TurnManager
            GameObject unitGO = new GameObject("Unit");
            BattleEntity unit = unitGO.AddComponent<BattleEntity>();
            unitGO.AddComponent<NetworkIdentity>();

            turnManager.RegisterParticipant(unit);
            await Task.Delay(500);

            // Вызываем команду завершения хода
            turnManager.CmdEndTurn();
            await Task.Delay(500);

            // Проверяем, что шкала хода юнита сброшена на сервере
            Assert.AreEqual(0, unit.turnGauge, "Шкала хода юнита должна сбрасываться после завершения хода.");

            // Проверяем, что шкала хода сброшена и у клиента
            await Task.Delay(500);
            Assert.AreEqual(0, TurnManager.Instance.GetParticipantById(turnManager.GetParticipants()[0]).turnGauge, "Сброс шкалы хода должен синхронизироваться между сервером и клиентами.");
        }

        [Test]
        public async Task TestRegisterAndUnregisterParticipantSync()
        {
            // Создаем юнит и регистрируем его
            GameObject unitGO = new GameObject("Unit");
            BattleEntity unit = unitGO.AddComponent<BattleEntity>();
            unitGO.AddComponent<NetworkIdentity>();

            turnManager.RegisterParticipant(unit);
            await Task.Delay(500);

            Assert.IsTrue(turnManager.SyncParticipants.Contains(unit.netId), "Юнит должен присутствовать в SyncParticipants после регистрации.");

            // Удаляем юнит из участников
            turnManager.UnregisterParticipant(unit);
            await Task.Delay(500);

            // Проверяем, что юнит удален с сервера
            Assert.IsFalse(turnManager.SyncParticipants.Contains(unit.netId), "Юнит должен быть удален из SyncParticipants после удаления.");

            // Проверяем, что клиент тоже получил обновленный список
            await Task.Delay(500);
            Assert.IsFalse(turnManager.GetParticipants().Contains(unit.netId), "SyncParticipants должен синхронизироваться при удалении юнита.");
        }
    }
}
#endif