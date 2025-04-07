using UnityEngine;
using System.Collections;
using System;

public class RPGStrategyGameManager : MonoBehaviour
{
    public enum GameState { PlayerTurn, EnemyTurn, GameOver }

    public GameState CurrentState { get; private set; } = GameState.PlayerTurn;
    public int CurrentTurn { get; private set; } = 1;
    
    [Header("Settings")]
    public KeyCode endTurnKey = KeyCode.LeftAlt;

    public HexGrid grid;

    public HexUI hexUI;  

    private void Start()
    {
        hexUI.gameUI.CurrentTurn = CurrentTurn;
        Debug.Log("Game started! Base Mode. Player's Turn.");
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
        hexUI.DisableAllUnitsUI();
        hexUI.gameUI.enabled = false;
        Debug.Log("Enemy's turn.");

        StartNewTurn();
    }

    private void StartNewTurn()
    {
        CurrentTurn++;
        CurrentState = GameState.PlayerTurn;
        hexUI.gameUI.enabled = true;
        hexUI.gameUI.CurrentTurn = CurrentTurn;
        grid.ResetUnitsStamina();
        grid.ClearPath();
        Debug.Log($"Turn {CurrentTurn}. Player's turn.");
    }

    public void CheckGameOver(bool isBaseDestroyed)
    {
        if (isBaseDestroyed)
        {
            CurrentState = GameState.GameOver;
        }
    }
}