using System;
using System.Collections.Generic;
using System.IO;
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

    public void CreateSquad(HexCell cell, List<PlayerCharacter> squadCharacters)
    {
        for (int i = 0; i < squadCharacters.Count; i++)
            availableCharacters.Remove(squadCharacters[i]);
        Grid.AddPlayerSquad(cell, UnityEngine.Random.Range(0f, 360f), squadCharacters, this);
    }

    public void AddCharacter(PlayerCharacter character)
    {
        character.SetupWeapons();
        characters.Add(character);
        availableCharacters.Add(character);
    }

    private int GetAvailableCharacterIndex(PlayerCharacter character)
    {
        for(int i = 0; i < characters.Count; i++)
        {
            if(characters[i] == character)
                return i;
        }
        return -1;
    }

    public int GetCharacterID(PlayerCharacter character)
    {
        for(int i = 0; i < characters.Count; i++)
        {
            if(characters[i] == character)
                return i;
        }
        return -1;
    }

    public PlayerCharacter GetCharacterByID(int id)
    {
        if(id < 0 || id > characters.Count)
            return null;
        
        return characters[id];
    }
	//============================================================================================================
	//                                       Сохранение и загрузка 
	//============================================================================================================
    public override void Save(BinaryWriter writer)
    {
        base.Save(writer);

        writer.Write(characters.Count);
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].Save(writer);
        }

        writer.Write(availableCharacters.Count);
        for (int i = 0; i < availableCharacters.Count; i++)
        {
            writer.Write(GetAvailableCharacterIndex(availableCharacters[i]));
        }

        inventory.Save(writer);
    }

    public override void Load(BinaryReader reader, HexGrid grid)
    {
        base.Load(reader, grid);

        int characterCount = reader.ReadInt32();
        for(int i = 0; i < characterCount; i++)
        {
            PlayerCharacter newCharacter = new();
            newCharacter.Load(reader);
            characters.Add(newCharacter);
        }

        int availableCharactersCount = reader.ReadInt32();
        for(int i = 0; i < availableCharactersCount; i++)
        {
            availableCharacters.Add(characters[reader.ReadInt32()]);
        }

        inventory.Load(reader);
    }
}
