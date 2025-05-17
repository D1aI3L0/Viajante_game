using NUnit.Framework;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveLoadTests
{
    [TestFixture]
    public class BaseSaveLoadTests
    {
        private readonly string testFilePath = Application.persistentDataPath + "/base_test_save.dat";

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
        public async Task TestSaveAndLoadBase()
        {
            await Task.Delay(1000);
            Assert.IsNotNull(Base.Instance, "Base.Instance не инициализирован перед сохранением!");

            using (BinaryWriter writer = new BinaryWriter(File.Open(testFilePath, FileMode.Create)))
            {
                Base.Instance.Save(writer);
            }

            Base newBaseInstance = Object.Instantiate(ScenePreload.grid.basePrefab);

            using (BinaryReader reader = new BinaryReader(File.Open(testFilePath, FileMode.Open)))
            {
                newBaseInstance.Load(reader, ScenePreload.grid);
            }

            Assert.AreEqual(Base.Instance.characters.Count, newBaseInstance.characters.Count, "Список персонажей должен совпадать после загрузки.");
            Assert.AreEqual(Base.Instance.availableCharacters.Count, newBaseInstance.availableCharacters.Count, "Список доступных персонажей должен совпадать после загрузки.");
            Assert.IsTrue(newBaseInstance.inventory != null, "Инвентарь должен загружаться корректно.");

            Object.DestroyImmediate(newBaseInstance);
        }
    }
}