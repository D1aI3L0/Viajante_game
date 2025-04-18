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

    private void Start()
    {
        UIReferences.gameUI.CurrentTurn = CurrentTurn;
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
        UIReferences.hexUI.DisableAllUnitsUI();
        UIReferences.gameUI.enabled = false;

        StartNewTurn();
    }

    private void StartNewTurn()
    {
        CurrentTurn++;
        CurrentState = GameState.PlayerTurn;
        UIReferences.gameUI.enabled = true;
        UIReferences.gameUI.CurrentTurn = CurrentTurn;
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
}