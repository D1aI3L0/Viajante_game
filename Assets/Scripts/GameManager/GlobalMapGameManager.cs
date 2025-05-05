using UnityEngine;
using System.Collections;
using System;

public class GlobalMapGameManager : MonoBehaviour
{
    public enum GameState { PlayerTurn, EnemyTurn, GameOver }

    public GameState CurrentState { get; private set; } = GameState.PlayerTurn;
    public int CurrentTurn { get; private set; } = 1;
    
    [Header("Settings")]
    public KeyCode endTurnKey = KeyCode.LeftAlt;

    public HexGrid grid;
    
    private int recruitCycle = 7;

    private void Start()
    {
        GameUI.Instance.CurrentTurn = CurrentTurn;
        RecruitingUI.Instance.UpdateRecruitingCharacters();
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
        HexUI.Instance.DisableAllUnitsUI();
        GameUI.Instance.enabled = false;

        StartNewTurn();
    }

    private void StartNewTurn()
    {
        CurrentTurn++;
        if(CurrentTurn%recruitCycle == 0)
            RecruitingUI.Instance.UpdateRecruitingCharacters();
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
}