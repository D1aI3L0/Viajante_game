using UnityEngine;

public class MainMenuUIController : MonoBehaviour
{
    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelSinglePlayer;
    [SerializeField] private GameObject panelMultiplayer;
    [SerializeField] private GameObject panelOptions;




    public void ShowMainMenuPanel()
    {
        panelMainMenu.SetActive(true);
        panelSinglePlayer.SetActive(false);
        panelMultiplayer.SetActive(false);
        panelOptions.SetActive(false);
    }

    public void ShowSinglePlayerPanel()
    {
        panelMainMenu.SetActive(false);
        panelSinglePlayer.SetActive(true);
        panelMultiplayer.SetActive(false);
        panelOptions.SetActive(false);
    }

    public void ShowMultiplayerPanel()
    {
        panelMainMenu.SetActive(false);
        panelSinglePlayer.SetActive(false);
        panelMultiplayer.SetActive(true);
        panelOptions.SetActive(false);
    }

    public void ShowOptionsPanel()
    {
        panelMainMenu.SetActive(false);
        panelSinglePlayer.SetActive(false);
        panelMultiplayer.SetActive(false);
        panelOptions.SetActive(true);
    }

    private void Start()
    {
        ShowMainMenuPanel();
    }

    // ------ Кнопки панели MainMenu
    public void OnExitButtonClicked()
    {
        Debug.Log("ExitButtonClicked");
    }

    // ------ Кнопки панели SinglePlayer
    public void OnLoadGameButtonClicked()
    {
        Debug.Log("LoadGameButtonClicked");
    }

    public void OnNewGameButtonClicked()
    {
        Debug.Log("NewGameButtonClicked");
    }

    // ------ Кнопки панели Multiplayer
    public void OnCreateSessionButtonClicked()
    {
        Debug.Log("CreateSessionButtonClicked");
    }

    public void OnConnectionSessionButtonClicked()
    {
        Debug.Log("ConnectionSessionButtonClicked");
    }

}
