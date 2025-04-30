using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainBaseUI : MonoBehaviour
{
    [Header("Основные элементы")]
    public GameObject UIContainer;
    public RectTransform buttonGroup;

    [Header("Кнопки")]
    public Button createSquadBtn;
    public Button workshopBtn;
    public Button upgradeBtn;

    [Header("Подменю")]
    public SquadCreationUI squadCreationMenu;
    public WorkshopUI workshopMenu;
    public GameObject upgradeMenu;

    private Base currentBase;

    void Start()
    {
        UIReferences.mainBaseUI = this;
        squadCreationMenu.Hide();
        if (workshopMenu) workshopMenu.Hide();
        if (upgradeMenu) upgradeMenu.SetActive(false);
        UIContainer.SetActive(false);
        enabled = false;
    }

    void Update()
    {
        createSquadBtn.interactable = currentBase.availableCharacters.Count > 0;
    }

    public void ShowForBase(Base playerBase)
    {
        enabled = true;
        currentBase = playerBase;
        UIContainer.SetActive(true);
        CloseAllSubmenus();

        createSquadBtn.interactable = playerBase.availableCharacters.Count > 0;
        workshopBtn.interactable = workshopMenu;
        upgradeBtn.interactable = upgradeMenu;
    }

    public void Hide()
    {
        enabled = false;
        CloseAllSubmenus();
        if (UIContainer.activeInHierarchy) UIContainer.SetActive(false);
    }

    public void Show()
    {
        enabled = true;
        CloseAllSubmenus();
        if (!UIContainer.activeInHierarchy) UIContainer.SetActive(true);
    }

    public void OpenSquadCreation()
    {
        CloseAllSubmenus();
        UIReferences.gameUI.enabled = false;
        squadCreationMenu.Show();
    }

    public void OpenWorkshopMenu()
    {
        CloseAllSubmenus();
        UIReferences.gameUI.enabled = false;
        workshopMenu.Show();
    }

    public void OpenUpgradeMenu()
    {
        CloseAllSubmenus();
        UIReferences.gameUI.enabled = false;
        upgradeMenu.SetActive(true);
    }

    void CloseAllSubmenus()
    {
        UIReferences.gameUI.enabled = true;
        if (squadCreationMenu) squadCreationMenu.Hide();
        if (workshopMenu) workshopMenu.Hide();
        if (upgradeMenu) upgradeMenu.SetActive(false);
    }
}