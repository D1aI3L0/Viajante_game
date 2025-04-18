using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Base : Unit
{
    public static Base Instance;
    public static Base basePrefab;
    public List<PlayerCharacter> characters;
    public List<PlayerCharacter> availableCharacters = new List<PlayerCharacter>();

    public void Initialise()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].UpdateStats();
            availableCharacters.Add(characters[i]);
        }
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
        Grid.AddPlayerSquad(Instantiate(Squad.squadPrefab), cell, UnityEngine.Random.Range(0f, 360f), characters, this);
    }
}
