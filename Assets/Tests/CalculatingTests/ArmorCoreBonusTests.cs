using NUnit.Framework;
using UnityEngine;

namespace CalculateTests
{
    [TestFixture]
    public class ArmorCoreCalculateTests
    {
        private ArmorCore armorCore;

        [SetUp]
        public void Setup()
        {
            armorCore = new ArmorCore();
            armorCore.Initialize();
        }

        [Test]
        public void TestInitialBonusValues()
        {
            Assert.AreEqual(0, armorCore.HealthBonus, "Начальный бонус к здоровью должен быть 0.");
            Assert.AreEqual(0, armorCore.DefenceBonus, "Начальный бонус к защите должен быть 0.");
            Assert.AreEqual(0, armorCore.EvasionBonus, "Начальный бонус к уклонению должен быть 0.");
        }

        [Test]
        public void TestUpgradeIncreasesBonus()
        {
            ArmorCoreUpgrade newUpgrade;
            bool upgradeSuccess = armorCore.TryUpgrade(out newUpgrade);

            Assert.IsTrue(upgradeSuccess, "Улучшение должно успешно добавляться.");

            switch (newUpgrade.statType)
            {
                case SurvivalStatType.Health:
                    Assert.Greater(armorCore.HealthBonus, 0, "Бонус к здоровью должен увеличиться.");
                    break;
                case SurvivalStatType.Defence:
                    Assert.Greater(armorCore.DefenceBonus, 0, "Бонус к защите должен увеличиться.");
                    break;
                case SurvivalStatType.Evasion:
                    Assert.Greater(armorCore.EvasionBonus, 0, "Бонус к уклонению должен увеличиться.");
                    break;
            }
        }

        [Test]
        public void TestBurnedUpgradeDoesNotAffectBonuses()
        {
            ArmorCoreUpgrade burnedUpgrade = new ArmorCoreUpgrade
            {
                gridPosition = new Vector2Int(1, 1),
                statType = SurvivalStatType.Health,
                bonusValue = 10,
                isBurned = true
            };

            armorCore.GetUpgrades().Add(burnedUpgrade);
            armorCore.CalculateBonuses();

            Assert.AreEqual(0, armorCore.HealthBonus, "Бонус к здоровью не должен увеличиваться после сгоревшего улучшения.");
        }
    }
}
