using System.Collections.Generic;

public enum SquadType
{
    Player,
    Enemy
}

public class Squad : Unit
{
    public static Squad squadPrefab;
    public List<Character> characters = new List<Character>();
    public SquadType squadType;
    private Base @base;

    public void Initialize(Base @base, List<PlayerCharacter> characters)
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

    public void Initialize(List<EnemyCharacter> characters)
    {
        if (characters != null)
        {
            this.characters.AddRange(characters);
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

    override public bool IsValidDestination(HexCell cell)
    {
        return base.IsValidDestination(cell) && (!cell.Unit || (cell.Unit is Squad currentCellSquad && currentCellSquad.squadType == SquadType.Enemy));
    }

    public override bool IsValidMove(HexCell cell)
    {
        return base.IsValidMove(cell) && (!cell.Unit || cell.Unit is Squad currentCellSquad && currentCellSquad.squadType != SquadType.Enemy || cell.Unit is Base);
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
                if(characters[i] is PlayerCharacter playerCharacter)
                    @base.availableCharacters.Add(playerCharacter);
            }
        }
        characters.Clear();
        Die();
    }
}