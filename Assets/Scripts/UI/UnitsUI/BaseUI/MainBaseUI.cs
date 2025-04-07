using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainBaseUI : MonoBehaviour
{
    [Header("Основные элементы")]
    public GameObject menuPanel;
    public RectTransform buttonGroup;
    public float slideSpeed = 5f;

    [Header("Кнопки")]
    public Button createSquadBtn;
    public Button manageBtn;
    public Button upgradeBtn;
    public Button closeBtn;

    [Header("Подменю")]
    public SquadCreationUI squadCreationMenu;
    public GameObject manageMenu;
    public GameObject upgradeMenu;

    private Base currentBase;

    [Header("Главное UI")]
    public HexUI hexUI;

    void Start()
    {
        squadCreationMenu.gameObject.SetActive(false);
        if (manageMenu) manageMenu.SetActive(false);
        if (upgradeMenu) upgradeMenu.SetActive(false);
        menuPanel.SetActive(false);
        enabled = false;
    }

    void Update()
    {
        createSquadBtn.interactable = currentBase.availableCharacters.Count > 0;
        if (Input.GetKeyDown(KeyCode.Escape))
            HideMenu();
        if (Input.GetKeyDown(KeyCode.Tab))
            ShowMenu();
    }

    public void ShowForBase(Base playerBase)
    {
        enabled = true;
        currentBase = playerBase;
        menuPanel.SetActive(true);
        CloseAllSubmenus();

        createSquadBtn.interactable = playerBase.availableCharacters.Count > 0;
        manageBtn.interactable = manageMenu;
        upgradeBtn.interactable = upgradeMenu;
    }

    public void HideMenu()
    {
        CloseAllSubmenus();
        if (menuPanel.activeInHierarchy) menuPanel.SetActive(false);
    }

    public void ShowMenu()
    {
        CloseAllSubmenus();
        if (!menuPanel.activeInHierarchy) menuPanel.SetActive(true);
    }

    public void OpenSquadCreation()
    {
        CloseAllSubmenus();
        hexUI.gameUI.enabled = false;
        squadCreationMenu.gameObject.SetActive(true);
        squadCreationMenu.Initialize(currentBase);
    }

    public void OpenManageMenu()
    {
        CloseAllSubmenus();
        hexUI.gameUI.enabled = false;
        manageMenu.SetActive(true);
    }

    public void OpenUpgradeMenu()
    {
        CloseAllSubmenus();
        hexUI.gameUI.enabled = false;
        upgradeMenu.SetActive(true);
    }

    void CloseAllSubmenus()
    {
        hexUI.gameUI.enabled = true;
        squadCreationMenu.gameObject.SetActive(false);
        if (manageMenu) manageMenu.SetActive(false);
        if (upgradeMenu) upgradeMenu.SetActive(false);
    }
}