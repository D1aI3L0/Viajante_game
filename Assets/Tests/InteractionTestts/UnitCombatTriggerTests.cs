using NUnit.Framework;
using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InteractionTests
{
    [TestFixture]
    public class UnitCombatTriggerTests
    {
        private Unit testUnit;
        private HexCell startCell;
        private HexCell enemyCell;
        private Squad enemySquad;
        private TestBattleConfirmationUI battleUI;

        [SetUp]
        public void Setup()
        {
            GameObject unitGO = new GameObject("TestUnit");
            testUnit = unitGO.AddComponent<Squad>();
            ((Squad)testUnit).squadType = SquadType.Player;

            GameObject enemySquadGO = new GameObject("EnemySquad");
            enemySquad = enemySquadGO.AddComponent<Squad>();
            enemySquad.squadType = SquadType.Enemy;

            startCell = new GameObject().AddComponent<HexCell>();
            startCell.Unit = testUnit;
            enemyCell = new GameObject().AddComponent<HexCell>();
            enemyCell.Unit = enemySquad;

            GameObject battleConfirmationUIGO = new GameObject("BattleUI");
            battleUI = battleConfirmationUIGO.AddComponent<TestBattleConfirmationUI>();
            BattleConfirmationUI.Instance = battleUI;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testUnit);
            Object.DestroyImmediate(enemyCell);
            Object.DestroyImmediate(enemySquad);
            Object.DestroyImmediate(battleUI);
            Object.DestroyImmediate(startCell);
        }

        [Test]
        public async Task TestCombatMenuTriggeredOnEnemyEncounter()
        {
            testUnit.Travel(new List<HexCell> { startCell, enemyCell });

            for (int i = 0; i < 50; i++)
            {
                if (battleUI.wasCalled)
                    break;
                await Task.Delay(100);
            }

            Assert.IsTrue(battleUI.wasCalled, "Меню боя должно было открыться при входе в клетку с врагом.");
        }
    }

    public class TestBattleConfirmationUI : BattleConfirmationUI
    {
        public bool wasCalled = false;

        protected override void Awake(){}

        public override void Show(Squad enemySquad, System.Action<bool> callback)
        {
            wasCalled = true;
            callback?.Invoke(true);
        }
    }
}