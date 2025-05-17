using System;
using System.IO;
using UnityEngine;


[Serializable]
public class EnemyCharacter : Character
{
    public int ID {get; private set;}
    public bool isElite;

    public WeaponParameters weaponParameters = new();
    public WeaponSkillSet skillSet = new();

    public void Initialize(EnemyTemplate enemyTemplate, int id)
    {
        ID = id;
        Initialise(enemyTemplate);
    }

    public void Initialise(EnemyTemplate enemyTemplate)
    {
        characterName = enemyTemplate.name;
        baseCharacterStats.maxHealth = enemyTemplate.enemyStats.characterParameters.maxHP;
        baseCharacterStats.defence = enemyTemplate.enemyStats.characterParameters.DEF;
        baseCharacterStats.evasion = enemyTemplate.enemyStats.characterParameters.EVA;
        baseCharacterStats.speed = enemyTemplate.enemyStats.characterParameters.SPD;
        baseCharacterStats.SPamount = enemyTemplate.enemyStats.characterParameters.SP;
        baseCharacterStats.SPmoveCost = enemyTemplate.enemyStats.characterParameters.SPmovecost;
        baseCharacterStats.SPregen = enemyTemplate.enemyStats.characterParameters.SPreg;
        baseCharacterStats.tount = enemyTemplate.enemyStats.characterParameters.PROV;

        weaponParameters.ATK = enemyTemplate.enemyStats.weaponParameters.ATK;
        weaponParameters.ACC = enemyTemplate.enemyStats.weaponParameters.ACC;
        weaponParameters.SE = enemyTemplate.enemyStats.weaponParameters.SE;
        weaponParameters.SEreg = enemyTemplate.enemyStats.weaponParameters.SEreg;
        weaponParameters.SEdec = enemyTemplate.enemyStats.weaponParameters.SEdec;
        weaponParameters.CRIT = enemyTemplate.enemyStats.weaponParameters.CRIT;

        UpdateStats();
    }

    private void UpdateStats()
    {
        currentCharacterStats.maxHealth = baseCharacterStats.maxHealth;
        currentCharacterStats.defence = baseCharacterStats.defence;
        currentCharacterStats.evasion = baseCharacterStats.evasion;
        currentCharacterStats.speed = baseCharacterStats.speed;
        currentCharacterStats.SPamount = baseCharacterStats.SPamount;
        currentCharacterStats.SPregen = baseCharacterStats.SPregen;
        currentCharacterStats.SPmoveCost = baseCharacterStats.SPmoveCost;
        currentCharacterStats.tount = baseCharacterStats.tount;
    }

    public override void Save(BinaryWriter writer)
    {
        base.Save(writer);
        writer.Write(ID);
    }

    public override void Load(BinaryReader reader)
    {
        base.Load(reader);
        ID = reader.ReadInt32();

        Initialise(EnemiesController.GetEnemyByID(ID));
    }
}
