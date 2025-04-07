using System.Collections.Generic;

public class Squad : Unit
{
    public static Squad squadPrefab;
    public List<Character> characters = new List<Character>();
    public Character.CharacterType squadType;
    private Base @base;

    public void Initialize(Base @base, List<Character> characters)
    {
        this.@base = @base;
        if (characters != null)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                this.characters.Add(characters[i]);
            }
        }
    }

    public override void Die()
    {
        for (int i = characters.Count - 1; i >= 0; i++)
        {
            characters[i].Die();
        }
        base.Die();
    }

    override public bool IsValidDestination(HexCell cell, bool useUnitCollision = true)
    {
        return base.IsValidDestination(cell) && (!useUnitCollision || !cell.HasUnit || (cell.Unit is Squad squad && squad.squadType != Character.CharacterType.Enemy) || cell.Unit is Base);
    }

    public static bool SquadValidDestionation(HexCell cell)
    {
        return UnitValidDestination(cell);
    }

    override public void ResetStamina()
    {
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].ResetStamina();
        }
        base.ResetStamina();
    }

    override public void Travel(List<HexCell> path)
    {
        for (int i = 0; i < characters.Count; i++)
        {
            characters[i].Stamina -= path[^1].Distance;
        }
        base.Travel(path);
    }

    public void ReturnToBase()
    {
        if (@base)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                @base.availableCharacters.Add(characters[i]);
            }
        }
        characters.Clear();
        Die();
    }
}