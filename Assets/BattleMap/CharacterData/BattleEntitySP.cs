using UnityEngine;
using System;

[Serializable]
public class BattleEntitySP : MonoBehaviour
{
    public BattleCell CurrentCell { get; set; }
    public event Action OnStatsChanged;

    public bool isActiveTurn = false;
    public float turnGauge = 0f;
    public Sprite unitIcon;

    public virtual bool IsEnemy => false;
    public virtual bool IsAlly => !IsEnemy;

    public int maxHP;
    private int _currentHP;
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

    public int baseDEF;
    public int currentDEF;
    public int baseEVA;
    public int currentEVA;
    public int baseSPD;
    public int currentSPD;

    public int maxSP;
    private int _currentSP;
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
    public int SPreg;
    public int SPmovecost;

    public void RaiseOnStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    public virtual void TakeDamage(float amount)
    {
        CurrentHP -= Mathf.RoundToInt(amount);
        Debug.Log($"{name} получает {amount:F1} урона. Остаток здоровья: {CurrentHP}");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Debug.Log($"{name} уничтожен!");
        }
    }

    public virtual int[] GetSkillIDs()
    {
        return new int[0];
    }

    public int currentCooldown;
}