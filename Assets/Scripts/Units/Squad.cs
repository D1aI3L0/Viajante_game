using System.Collections.Generic;
using System.IO;

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

    public PlayerCharacter[] ToPlayerCharactersArray()
    {
        PlayerCharacter[] playerCharacters = new PlayerCharacter[characters.Count];
        for(int i = 0; i < characters.Count; i++)
        {
            playerCharacters[i] = (PlayerCharacter)characters[i];
        }
        return playerCharacters;
    }

    public EnemyCharacter[] ToEnemyCharactersArray()
    {
        EnemyCharacter[] enemyCharacters = new EnemyCharacter[characters.Count];
        for(int i = 0; i < characters.Count; i++)
        {
            enemyCharacters[i] = (EnemyCharacter)characters[i];
        }
        return enemyCharacters;
    }
    //============================================================================================================
    //                                       Сохранение и загрузка 
    //============================================================================================================
    public override void Save(BinaryWriter writer)
    {
        base.Save(writer);

        if (squadType == SquadType.Player)
        {
            SavePlayerSquad(writer);
        }
        else
        {
            SaveEnemySquad(writer);
        }
    }

    private void SavePlayerSquad(BinaryWriter writer)
    {
        writer.Write(characters.Count);
        foreach (Character character in characters)
        {
            writer.Write(Base.Instance.GetCharacterID((PlayerCharacter)character));
        }
        inventory.Save(writer);
    }

    private void SaveEnemySquad(BinaryWriter writer)
    {
        writer.Write(characters.Count);
        foreach (Character character in characters)
        {
            ((EnemyCharacter)character).Save(writer);
        }
    }

    public override void Load(BinaryReader reader, HexGrid grid)
    {
        base.Load(reader, grid);

        if (squadType == SquadType.Player)
        {
            LoadPlayerSquad(reader);
        }
        else
        {
            LoadEnemySquad(reader);
        }
    }

    private void LoadPlayerSquad(BinaryReader reader)
    {
        @base = Base.Instance;

        int charactersCount = reader.ReadInt32();
        for(int i = 0; i < charactersCount; i++)
        {
            characters.Add(@base.GetCharacterByID(reader.ReadInt32()));
        }

        foreach(Character character in characters)
        {
            Base.Instance.availableCharacters.Remove((PlayerCharacter)character);
        }

        inventory = new();
        inventory.Load(reader);
    }

    private void LoadEnemySquad(BinaryReader reader)
    {
        @base = null;
        inventory = null;
        int enemiesCount = reader.ReadInt32();
        for (int i = 0; i < enemiesCount; i++)
        {
            EnemyCharacter enemy = new();
            enemy.Load(reader);
            characters.Add(enemy);
        }
    }
}