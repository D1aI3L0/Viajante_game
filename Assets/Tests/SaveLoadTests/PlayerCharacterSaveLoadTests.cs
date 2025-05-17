using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace SaveLoadTests
{
    [TestFixture]
    public class PlayerCharacterSaveLoadTests
    {
        private PlayerCharacter character;
        private readonly string testFilePath = Application.persistentDataPath + "/playercharacter_test_save.dat";

        [SetUp]
        public void Setup()
        {
            _ = ScenePreload.Setup();

            character = new PlayerCharacter();
            character.characterClass = CharacterClass.WarriorZastupnik;
            character.Initialize(RecruitingController.GetCharacterTemplate(character.characterClass));

            character.currentHealth = 80;

            TraitData testTraitData = Resources.Load<TraitData>("Test_Trait");
            character.AddTrait(testTraitData);
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
        public void TestSaveAndLoadPlayerCharacter()
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(testFilePath, FileMode.Create)))
            {
                character.Save(writer);
            }

            PlayerCharacter loadedCharacter = new PlayerCharacter();
            using (BinaryReader reader = new BinaryReader(File.Open(testFilePath, FileMode.Open)))
            {
                loadedCharacter.Load(reader);
            }

            Assert.AreEqual(character.characterClass, loadedCharacter.characterClass, "Класс персонажа должен совпадать после загрузки.");

            Assert.AreEqual(character.currentHealth, loadedCharacter.currentHealth, "Здоровье должно совпадать после загрузки.");

            Assert.AreEqual(character.GetAvailableWeapons().Count, loadedCharacter.GetAvailableWeapons().Count, "Список оружия должен совпадать после загрузки.");

            Assert.AreEqual(character.GetTraits(TraitType.Positive).Count, loadedCharacter.GetTraits(TraitType.Positive).Count, "Положительные трейты должны совпадать.");
            Assert.AreEqual(character.GetTraits(TraitType.Negatine).Count, loadedCharacter.GetTraits(TraitType.Negatine).Count, "Негативные трейты должны совпадать.");
        }
    }
}