using System.Collections.Generic;

public enum SquadType
{
    Player,
    Enemy
}

public class Squad : Unit
{
    public static Squad playerSquadPrefab, enemySquadPrefab;
    public List<Character> characters = new();
    public SquadType squadType;
    private Base @base;
    public Inventory inventory;

    public override HexCell Location
    {
        get => base.Location;
        set
        {
            if (location && squadType == SquadType.Player)
            {
                Grid.DecreaseVisibility(location, VisionRange);
                Grid.IncreaseVisibility(value, VisionRange);
            }

            base.Location = value;
        }
    }

    protected override void OnEnable()
    {
        if (location)
        {
            if (currentTravelLocation)
            {
                Grid.IncreaseVisibility(location, VisionRange);
                Grid.DecreaseVisibility(currentTravelLocation, VisionRange);
            }
            base.OnEnable();
        }
    }

    public void Initialize(Base @base, List<PlayerCharacter> characters)
    {
        squadType = SquadType.Player;
        this.@base = @base;
        this.characters.AddRange(characters);
        inventory = new();
    }

    public void Initialize(List<EnemyCharacter> characters)
    {
        squadType = SquadType.Enemy;
        @base = null;
        this.characters.AddRange(characters);
        inventory = null;
    }

    public override void Die()
    {
        if (squadType != SquadType.Enemy && location)
        {
            Grid.DecreaseVisibility(location, VisionRange);
        }

        Grid.RemoveSquad(this);
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

    public void ReturnToBase()
    {
        if (@base && squadType == SquadType.Player)
        {
            for (int i = 0; i < characters.Count; i++)
            {
                if (characters[i] is PlayerCharacter playerCharacter)
                    @base.availableCharacters.Add(playerCharacter);
            }
            @base.inventory.AddRange(inventory);
        }
        characters.Clear();
        Die();
    }
}