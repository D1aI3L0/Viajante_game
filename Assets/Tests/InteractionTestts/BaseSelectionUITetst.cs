using NUnit.Framework;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InteractionTests
{
    [TestFixture]
    public class BaseSelectionUITests
    {
        private GameUI gameUI;
        private HexCell baseCell;
        private Base testBase;
        private TestMainBaseUI testMainBaseUI;

        [SetUp]
        public async Task Setup()
        {
            _ = ScenePreload.Setup();

            await Task.Delay(1000);

            gameUI = Object.FindFirstObjectByType<GameUI>();
            Assert.IsNotNull(gameUI, "GameUI не загружен!");

            GameObject baseGO = new GameObject("TestBase");
            testBase = baseGO.AddComponent<Base>();
            baseCell = new GameObject().AddComponent<HexCell>();
            baseCell.Unit = testBase;

            gameUI.grid = Object.FindFirstObjectByType<HexGrid>();
            Assert.IsNotNull(gameUI.grid, "HexGrid не загружен!");

            GameObject testMainBaseUIGO = new GameObject();
            testMainBaseUI = testMainBaseUIGO.AddComponent<TestMainBaseUI>();
            MainBaseUI.Instance = testMainBaseUI;
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(testMainBaseUI);
            Object.DestroyImmediate(testBase);
            Object.DestroyImmediate(baseCell);
        }

        [Test]
        public async Task TestBaseSelectionOpensBaseUI()
        {
            gameUI.currentCell = baseCell;
            gameUI.DoTestSelection();

            await Task.Delay(500);

            Assert.IsTrue(testMainBaseUI.wasCalled, "Меню базы должно было открыться при выборе.");
        }
    }


    public class TestMainBaseUI : MainBaseUI
    {
        public bool wasCalled = false;

        protected override void Start(){}

        protected override void Update(){}

        public override void ShowForBase(Base baseInstance)
        {
            wasCalled = true;
        }
    }
}