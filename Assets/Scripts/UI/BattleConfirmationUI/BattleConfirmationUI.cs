using System;
using TMPro;
using UnityEngine;

public class BattleConfirmationUI : MonoBehaviour
{
    public static BattleConfirmationUI Instance;
    public GameObject UIContainer;

    public TMP_Text info;

    private Action<bool> combatCallback;

    protected virtual void Awake()
    {
        Instance = this;
        Hide();
    }

    public virtual void Show(Squad enemy, Action<bool> action)
    {
        enabled = true;
        UIContainer.SetActive(true);
        combatCallback = action;
        info.text = $"Founded {enemy.name}!\nEnter battle?";
    }

    public virtual void Hide()
    {
        enabled = false;
        UIContainer.SetActive(false);
    }

    public void HandleCombatChoice(bool confirmed)
    {
        Time.timeScale = 1f;
        combatCallback?.Invoke(confirmed);
        Hide();
    }

    public static bool IsCombatDialogActive => Instance.UIContainer.activeSelf;
}