using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class GlobalMapGameManager : MonoBehaviour
{
    public static GlobalMapGameManager Instance;

    [SerializeField] private GameParameters gameParameters;
    [SerializeField] private BattleConfig battleConfig;
    [SerializeField] private CharacterDataTransferParameters characterTransfer;
    [SerializeField] private EnemyTransferSystem enemyTransfer;

    public enum GameState { PlayerTurn, EnemyTurn, GameOver }

    public GameState CurrentState { get; private set; } = GameState.PlayerTurn;
    public int CurrentTurn { get; private set; } = 1;

    [Header("Settings")]
    public KeyCode endTurnKey = KeyCode.LeftAlt;

    public HexGrid grid;

    private const int recruitCycle = 7;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (gameParameters.isNewGame)
        {
            NewMapMenu.Instance.CreateLargeMap();
            GameUI.Instance.CurrentTurn = CurrentTurn;
            RecruitingController.Instance.UpdateRecruitingCharacters();
            grid.AddBase(grid.GetAvailableBaseLocation(), UnityEngine.Random.Range(0f, 360f));
            HexMapCamera.MoveToBase();
            EnemiesController.Instance.OnTurnEnd(0);
        }
        else
        {
            SaveLoadMenu.Instance.LoadGame(gameParameters.gameName);
            GameUI.Instance.CurrentTurn = CurrentTurn;
        }
    }

    private void Update()
    {
        if (CurrentState != GameState.PlayerTurn) return;

        if (Input.GetKeyDown(endTurnKey))
        {
            EndPlayerTurn();
        }
    }

    public void EndPlayerTurn()
    {
        if (CurrentState != GameState.PlayerTurn) return;

        CurrentState = GameState.EnemyTurn;
        HexUI.Instance.HideAllUIs();
        GameUI.Instance.enabled = false;

        EnemiesController.Instance.OnTurnEnd(CurrentTurn);

        StartNewTurn();
    }

    private void StartNewTurn()
    {
        CurrentTurn++;
        if (CurrentTurn % recruitCycle == 0)
            RecruitingController.Instance.UpdateRecruitingCharacters();
        CurrentState = GameState.PlayerTurn;
        GameUI.Instance.enabled = true;
        GameUI.Instance.CurrentTurn = CurrentTurn;
        grid.ResetUnitsStamina();
        grid.ClearPath();
    }

    public void CheckGameOver(bool isBaseDestroyed)
    {
        if (isBaseDestroyed)
        {
            CurrentState = GameState.GameOver;
        }
    }

    public string GetGameName()
    {
        return gameParameters.gameName;
    }

    public void SetBattleParametres(Squad playerSquad, Squad enemySquad)
    {
        SaveLoadMenu.Instance.SaveGame(gameParameters.gameName);

        characterTransfer.numberOfCharacters = playerSquad.characters.Count;
        characterTransfer.playerCharacterData = playerSquad.ToPlayerCharactersArray();

        enemyTransfer.enemiesCount = enemySquad.characters.Count;
        enemyTransfer.enemyCharacters = enemySquad.ToEnemyCharactersArray();

        battleConfig.enemyCount = enemySquad.characters.Count;
    }

    public void Save(BinaryWriter writer)
    {
        writer.Write(CurrentTurn);
    }

    public void Load(BinaryReader reader)
    {
        CurrentTurn = reader.ReadInt32();
    }
}