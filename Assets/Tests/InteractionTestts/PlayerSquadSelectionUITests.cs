using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InteractionTests
{
    [TestFixture]
    public class PlayerSquadSelectionUITests
    {
        private GameUI gameUI;
        private HexCell squadCell;
        private Squad testSquad;
        private TestPlayerSquadUI testPlayerSquadUI;

        [SetUp]
        public async Task Setup()
        {
            _ = ScenePreload.Setup();

            await Task.Delay(1000);

            gameUI = Object.FindFirstObjectByType<GameUI>();
            Assert.IsNotNull(gameUI, "GameUI не загружен!");

            GameObject baseGO = new GameObject("TestBase");
            testSquad = baseGO.AddComponent<Squad>();
            testSquad.squadType = SquadType.Player;
            squadCell = new GameObject().AddComponent<HexCell>();
            squadCell.Unit = testSquad;

            gameUI.grid = Object.FindFirstObjectByType<HexGrid>();
            Assert.IsNotNull(gameUI.grid, "HexGrid не загружен!");

            GameObject testMainBaseUIGO = new GameObject();
            testPlayerSquadUI = testMainBaseUIGO.AddComponent<TestPlayerSquadUI>();
            testMainBaseUIGO.SetActive(true);
            PlayerSquadUI.Instance = testPlayerSquadUI;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testPlayerSquadUI);
            Object.DestroyImmediate(testSquad);
            Object.DestroyImmediate(squadCell);
        }

        [Test]
        public async Task TestBaseSelectionOpensBaseUI()
        {
            gameUI.currentCell = squadCell;
            gameUI.DoTestSelection();

            await Task.Delay(500);

            Assert.IsTrue(testPlayerSquadUI.wasCalled, "Меню базы должно было открыться при выборе.");
        }
    }


    public class TestPlayerSquadUI : PlayerSquadUI
    {
        public bool wasCalled = false;

        protected override void Awake(){}

        public override void ShowForSquad(Squad squad)
        {
            wasCalled = true;
        }
    }
}