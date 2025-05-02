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
    public Inventory inventory;

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
        inventory = new();
    }

    public void Initialize(List<EnemyCharacter> characters)
    {
        if (characters != null)
        {
            this.characters.AddRange(characters);
        }
        inventory = null;
    }

    public override void Die()
    {
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
        base.ResetStamina();
    }

    override public void Travel(List<HexCell> path)
    {
        base.Travel(path);
    }

    public void ReturnToBase()
    {
        if (@base && squadType == SquadType.Player)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if(characters[i] is PlayerCharacter playerCharacter)
                    @base.availableCharacters.Add(playerCharacter);
            }
            @base.inventory.AddRange(inventory);
        }
        characters.Clear();
        Die();
    }
}