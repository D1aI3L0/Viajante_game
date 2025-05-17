using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace SaveLoadTests
{
    [TestFixture]
    public class HexGridSaveLoadTests
    {
        private readonly string testFilePath = Application.persistentDataPath + "/hexgrid_test_save.dat";

        [SetUp]
        public void Setup()
        {
            _ = ScenePreload.Setup();
        }

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(testFilePath))
            {
                File.Delete(testFilePath);
            }
        }

        [Test]
        public void TestSaveAndLoadHexGrid()
        {
            Assert.IsNotNull(ScenePreload.grid, "Объект карты не существует");
            using (BinaryWriter writer = new BinaryWriter(File.Open(testFilePath, FileMode.Create)))
            {
                ScenePreload.grid.Save(writer);
            }

            GameObject newHexGridGO = new GameObject("HexGrid_Loaded");
            newHexGridGO.SetActive(false);
            HexGrid newHexGrid = newHexGridGO.AddComponent<HexGrid>();
            newHexGrid.skipAutoinitialization = true;
            newHexGrid.chunkPrefab = ScenePreload.grid.chunkPrefab;
            newHexGrid.basePrefab = ScenePreload.grid.basePrefab;
            newHexGrid.playerSquadPrefab = ScenePreload.grid.playerSquadPrefab;
            newHexGrid.enemySquadPrefab = ScenePreload.grid.enemySquadPrefab;
            newHexGrid.cellPrefab = ScenePreload.grid.cellPrefab;
            newHexGrid.cellLabelPrefab = ScenePreload.grid.cellLabelPrefab;
            newHexGrid.biomePrefab = ScenePreload.grid.biomePrefab;
            newHexGrid.settlementPrefab = ScenePreload.grid.settlementPrefab;
            newHexGridGO.SetActive(true);

            using (BinaryReader reader = new BinaryReader(File.Open(testFilePath, FileMode.Open)))
            {
                newHexGrid.Load(reader, 10);
            }

            Assert.AreEqual(ScenePreload.grid.cellCountX, newHexGrid.cellCountX, "Ширина карты должна совпадать после загрузки.");
            Assert.AreEqual(ScenePreload.grid.cellCountZ, newHexGrid.cellCountZ, "Высота карты должна совпадать после загрузки.");

            Assert.AreEqual(ScenePreload.grid.GetBiomesCount(), newHexGrid.GetBiomesCount(), "Количество биомов должно совпадать после загрузки.");

            Assert.AreEqual(ScenePreload.grid.GetSettlements().Count, newHexGrid.GetSettlements().Count, "Количество поселений должно совпадать после загрузки.");

            Assert.AreEqual(ScenePreload.grid.GetSquads(SquadType.Enemy).Count, newHexGrid.GetSquads(SquadType.Enemy).Count, "Количество отрядов врага должно совпадать после загрузки.");
            Assert.AreEqual(ScenePreload.grid.GetSquads(SquadType.Player).Count, newHexGrid.GetSquads(SquadType.Player).Count, "Количество отрядов игрока должно совпадать после загрузки.");

            Object.DestroyImmediate(newHexGridGO);
        }
    }
}