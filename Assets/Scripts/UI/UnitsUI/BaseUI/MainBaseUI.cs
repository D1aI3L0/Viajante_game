using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainBaseUI : MonoBehaviour
{
    [Header("Основные элементы")]
    public GameObject UIContainer;
    public RectTransform buttonGroup;
    public float slideSpeed = 5f;

    [Header("Кнопки")]
    public Button createSquadBtn;
    public Button workshopBtn;
    public Button upgradeBtn;

    [Header("Подменю")]
    public SquadCreationUI squadCreationMenu;
    public GameObject workshopMenu;
    public GameObject upgradeMenu;

    private Base currentBase;

    void Start()
    {
        UIReferences.mainBaseUI = this;
        squadCreationMenu.gameObject.SetActive(false);
        if (workshopMenu) workshopMenu.SetActive(false);
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
        squadCreationMenu.gameObject.SetActive(true);
        squadCreationMenu.Initialize(currentBase);
    }

    public void OpenManageMenu()
    {
        CloseAllSubmenus();
        UIReferences.gameUI.enabled = false;
        workshopMenu.SetActive(true);
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
        squadCreationMenu.gameObject.SetActive(false);
        if (workshopMenu) workshopMenu.SetActive(false);
        if (upgradeMenu) upgradeMenu.SetActive(false);
    }
}