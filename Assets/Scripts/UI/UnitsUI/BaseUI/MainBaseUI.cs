using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainBaseUI : MonoBehaviour
{
    public static MainBaseUI Instance;

    [Header("Основные элементы")]
    public GameObject UIContainer;
    public RectTransform buttonGroup;

    [Header("Кнопки")]
    public Button createSquadBtn;
    public Button workshopBtn;
    public Button upgradeBtn;
    public Button recruitmentgBtn;

    [Header("Подменю")]
    public SquadCreationUI squadCreationMenu;
    public WorkshopUI workshopMenu;
    public RecruitingUI recruitmentMenu;
    public GameObject upgradeMenu;

    private Base currentBase;

    void Start()
    {
        Instance = this;
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
        recruitmentgBtn.interactable = recruitmentMenu;
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
        GameUI.Instance.enabled = false;
        squadCreationMenu.Show();
    }

    public void OpenWorkshopMenu()
    {
        CloseAllSubmenus();
        GameUI.Instance.enabled = false;
        workshopMenu.Show();
    }

    public void OpenRecruitmentMenu()
    {
        CloseAllSubmenus();
        GameUI.Instance.enabled = false;
        recruitmentMenu.Show();
    }

    public void OpenUpgradeMenu()
    {
        CloseAllSubmenus();
        GameUI.Instance.enabled = false;
        upgradeMenu.SetActive(true);
    }

    void CloseAllSubmenus()
    {
        GameUI.Instance.enabled = true;
        if (squadCreationMenu) squadCreationMenu.Hide();
        if (workshopMenu) workshopMenu.Hide();
        if (recruitmentMenu) recruitmentMenu.Hide();
        if (upgradeMenu) upgradeMenu.SetActive(false);
    }
}