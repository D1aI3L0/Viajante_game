using Mirror;
using UnityEngine;
using System;

[Serializable]
public class BattleEntityMP : NetworkBehaviour
{
    public BattleCell currentCell;
    public event Action OnStatsChanged;

    [SyncVar] public bool isActiveTurn;
    [SyncVar] public float turnGauge;
    public Sprite unitIcon;

    public virtual bool IsEnemy => false;
    public virtual bool IsAlly => !IsEnemy;

    [SyncVar] public int maxHP;
    [SyncVar(hook = nameof(OnHealthChanged))] private int _currentHP;
    public int CurrentHP
    {
        get => _currentHP;
        set
        {
            if (_currentHP != value)
            {
                _currentHP = value;
                OnStatsChanged?.Invoke();
            }
        }
    }

    [SyncVar] public int baseDEF;
    [SyncVar] public int currentDEF;
    [SyncVar] public int baseEVA;
    [SyncVar] public int currentEVA;
    [SyncVar] public int baseSPD;
    [SyncVar] public int currentSPD;

    [SyncVar] public int maxSP;
    [SyncVar(hook = nameof(OnSPChanged))] private int _currentSP;
    public int CurrentSP
    {
        get => _currentSP;
        set
        {
            if (_currentSP != value)
            {
                _currentSP = value;
                Debug.Log($"{name} Updated CurrentSP: {_currentSP}");
                OnStatsChanged?.Invoke();
            }
        }
    }
    [SyncVar] public int SPreg;
    [SyncVar] public int SPmovecost;

    public void RaiseOnStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    [Server]
    public virtual void TakeDamage(float amount)
    {
        CurrentHP -= Mathf.RoundToInt(amount);
        Debug.Log($"{name} получает {amount:F1} урона. Остаток здоровья: {CurrentHP}");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Debug.Log($"{name} уничтожен!");
            RpcHandleDeath();
        }
    }

    [ClientRpc]
    private void RpcHandleDeath()
    {
        Debug.Log($"{name} уничтожен (Client-side).");
    }

    public virtual int[] GetSkillIDs()
    {
        return new int[0];
    }

    [SyncVar(hook = nameof(OnCooldownChanged))] public int currentCooldown;

    private void OnCooldownChanged(int oldValue, int newValue)
    {
        Debug.Log($"Cooldown изменился на: {newValue}");
    }

    [Command]
    public void CmdResetCooldown(SkillAsset skill)
    {
        skill.ResetCooldown();
        currentCooldown = skill.currentCooldown;
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        Debug.Log($"HP изменился: {newValue}");
    }

    private void OnSPChanged(int oldValue, int newValue)
    {
        Debug.Log($"SP изменился: {newValue}");
    }
}