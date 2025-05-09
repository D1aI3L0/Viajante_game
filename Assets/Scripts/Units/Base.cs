using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class Base : Unit
{
    public static Base Instance;
    public static Base basePrefab;
    public List<PlayerCharacter> characters = new();
    public List<PlayerCharacter> availableCharacters = new();

    public Inventory inventory = new();

    public void Initialise()
    {
        Instance = this;
    }

    override public bool IsValidDestination(HexCell cell)
    {
        return base.IsValidDestination(cell) && !cell.HasUnit;
    }

    public override bool IsValidMove(HexCell cell)
    {
        return base.IsValidMove(cell) && (!cell.Unit || cell.Unit is Squad currentCellSquad && currentCellSquad.squadType != SquadType.Enemy);
    }

    public void CreateSquad(HexCell cell, List<PlayerCharacter> characters)
    {
        for (int i = 0; i < characters.Count; i++)
            availableCharacters.Remove(characters[i]);
        Grid.AddPlayerSquad(cell, UnityEngine.Random.Range(0f, 360f), characters, this);
    }

    public void AddCharacter(PlayerCharacter character)
    {
        character.SetupWeapons();
        characters.Add(character);
        availableCharacters.Add(character);
    }
}
