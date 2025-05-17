using NUnit.Framework;
using UnityEngine;

namespace CalculateTests
{
    [TestFixture]
    public class PlayerCharacterStatsCalculateTests
    {
        private PlayerCharacter character;

        [SetUp]
        public void Setup()
        {
            character = new PlayerCharacter();
            character.Initialize();

            // 🔥 Устанавливаем параметры персонажа
            CharacterParameters paramsData = new CharacterParameters
            {
                maxHP = 100,
                DEF = 10,
                EVA = 5,
                SP = 50,
                SPreg = 5,
                SPmovecost = 2,
                SPD = 15,
                PROV = 0
            };
            character.InitializeStats(paramsData);
        }

        [Test]
        public void TestBaseStatsInitialization()
        {
            // 🔥 Проверяем базовые характеристики
            Assert.AreEqual(100, character.baseCharacterStats.maxHealth, "Начальное здоровье должно быть 100.");
            Assert.AreEqual(10, character.baseCharacterStats.defence, "Начальная защита должна быть 10.");
            Assert.AreEqual(5, character.baseCharacterStats.evasion, "Начальное уклонение должно быть 5.");
            Assert.AreEqual(15, character.baseCharacterStats.speed, "Начальная скорость должна быть 15.");
        }

        [Test]
        public void TestStatsRecalculationWithTrait()
        {
            TraitData traitData = new TraitData
            {
                effects = new TraitEffect[]
                {
                new TraitEffect { statType = StatType.Health, effectType = TraitEffectType.Additive, value = 20 },
                new TraitEffect { statType = StatType.Defence, effectType = TraitEffectType.Multiplicative, value = 50 }
                }
            };

            character.AddTrait(traitData);
            character.RecalculateStats();

            // 🔥 Проверяем, что здоровье увеличилось на 20
            Assert.AreEqual(120, character.currentCharacterStats.maxHealth, "Здоровье должно увеличиться на 20.");

            // 🔥 Проверяем, что защита увеличилась на 50%
            Assert.AreEqual(15, character.currentCharacterStats.defence, "Защита должна увеличиться на 50%.");
        }

        [Test]
        public void TestArmorCoreBonuses()
        {
            character.equipment.armorCore = new ArmorCore();
            character.equipment.armorCore.Initialize();
            character.equipment.armorCore.HealthBonus = 10; // +10% здоровья

            character.RecalculateStats();

            // 🔥 Проверяем, что бонусы брони применяются
            Assert.AreEqual(110, character.currentCharacterStats.maxHealth, "Бонус брони должен дать +10% здоровья.");
        }
    }
}