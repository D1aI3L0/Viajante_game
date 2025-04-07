using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Base : Unit
{
    public static Base basePrefab;
    public List<Character> characters;
    public List<Character> availableCharacters = new List<Character>();

    public void Initialise()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            availableCharacters.Add(characters[i]);
        }
    }

    override public bool IsValidDestination(HexCell cell, bool useUnitCollision = true)
    {
        return base.IsValidDestination(cell) && (!useUnitCollision || !cell.HasUnit || (cell.Unit is Squad squad && squad.squadType != Character.CharacterType.Enemy));
    }

    public void CreateSquad(HexCell cell, List<Character> characters)
    {
        for (int i = 0; i < characters.Count; i++)
            availableCharacters.Remove(characters[i]);
        Grid.AddSquad(Instantiate(Squad.squadPrefab), cell, UnityEngine.Random.Range(0f, 360f), Character.CharacterType.Player, characters, this);
    }
}
