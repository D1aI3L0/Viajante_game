using System.Collections.Generic;
using UnityEngine;

public class WorkshopUI : MonoBehaviour
{
    public static WorkshopUI Instance { get; private set; }

    public GameObject UIContainer;
    public EquipmentUpgradeUI equipmentUpgradeUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        CloseAllSubmenus();
    }

    public void Show()
    {
        enabled = true;
        UIContainer.SetActive(true);
        CloseAllSubmenus();
    }

    public void Hide()
    {
        GameUI.Instance.enabled = true;
        enabled = false;
        UIContainer.SetActive(false);
        CloseAllSubmenus();
    }

    public void OpenUpgradeUI()
    {
        CloseAllSubmenus();
        equipmentUpgradeUI.Show();
    }

    private void CloseAllSubmenus()
    {
        equipmentUpgradeUI.Hide();
    }
}