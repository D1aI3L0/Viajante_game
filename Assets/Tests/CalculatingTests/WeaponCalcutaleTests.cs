using NUnit.Framework;
using System.IO;
using UnityEngine;

namespace CalculateTests
{
    [TestFixture]
    public class WeaponCalculateTests
    {
        private Weapon weapon;

        [SetUp]
        public void Setup()
        {
            weapon = new Weapon();
            WeaponParameters parameters = new WeaponParameters { ATK = 50, ACC = 80, CRIT = 15, SE = 30 };
            WeaponSkillSet skillSet = new WeaponSkillSet();

            weapon.Initialize(parameters, skillSet);
        }

        [TearDown]
        public void TearDown()
        {

        }

        [Test]
        public void TestWeaponInitialization()
        {
            Assert.AreEqual(50, weapon.weaponParameters.ATK, "ATK оружия должен быть 50.");
            Assert.AreEqual(80, weapon.weaponParameters.ACC, "ACC оружия должен быть 80.");
            Assert.AreEqual(15, weapon.weaponParameters.CRIT, "CRIT оружия должен быть 15.");
            Assert.AreEqual(30, weapon.weaponParameters.SE, "SE оружия должен быть 30.");
        }

        [Test]
        public void TestTryUpgrade()
        {
            bool success = weapon.TryUpgrade(out WeaponUpgradeRune upgrade);

            Assert.IsTrue(success, "Улучшение должно успешно примениться.");
            Assert.IsNotNull(upgrade, "Улучшение не должно быть null после применения.");
        }
    }
}