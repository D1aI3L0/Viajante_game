using System;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIController : MonoBehaviour
{
    [SerializeField] private GameObject panelMainMenu;
    [SerializeField] private GameObject panelSinglePlayer;
    [SerializeField] private GameObject panelMultiplayer;
    [SerializeField] private GameObject panelOptions;
    [SerializeField] private GameObject panelCreation;
    [SerializeField] private GameObject panelLoad;

    [SerializeField] private TMP_InputField nameInput;

    [SerializeField] private SaveLoadItem saveLoadItemPrefab;
    [SerializeField] private Transform gamesContainer;
    [SerializeField] private TMP_Text selectedGameName;

    [SerializeField] private GameParameters gameParameters;


    public void ShowMainMenuPanel()
    {
        CloseAllPanels();
        panelMainMenu.SetActive(true);
    }

    public void ShowSinglePlayerPanel()
    {
        CloseAllPanels();
        panelSinglePlayer.SetActive(true);
    }

    public void ShowMultiplayerPanel()
    {
        CloseAllPanels();
        panelMultiplayer.SetActive(true);
    }

    public void ShowOptionsPanel()
    {
        CloseAllPanels();
        panelOptions.SetActive(true);
    }

    public void ShowCreationPanel()
    {
        CloseAllPanels();
        nameInput.text = "";
        panelCreation.SetActive(true);
    }

    public void ShowLoadPanel()
    {
        CloseAllPanels();
        FillList();
        selectedGameName.text = "";
        panelLoad.SetActive(true);
    }

    private void CloseAllPanels()
    {
        panelMainMenu.SetActive(false);
        panelSinglePlayer.SetActive(false);
        panelMultiplayer.SetActive(false);
        panelOptions.SetActive(false);
        panelCreation.SetActive(false);
        panelLoad.SetActive(false);
    }

    private void FillList()
	{
		for (int i = 0; i < gamesContainer.childCount; i++)
		{
			Destroy(gamesContainer.GetChild(i).gameObject);
		}

		string[] paths = Directory.GetFiles(Application.persistentDataPath, "*.map");
		Array.Sort(paths);
		for (int i = 0; i < paths.Length; i++)
		{
			SaveLoadItem item = Instantiate(saveLoadItemPrefab);
			item.Initialize(this);
			item.MapName = System.IO.Path.GetFileNameWithoutExtension(paths[i]);
			item.transform.SetParent(gamesContainer, false);
		}
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
        if (selectedGameName.text != "")
        {
            gameParameters.gameName = selectedGameName.text;
            gameParameters.isNewGame = false;
            SceneManager.LoadScene("GlobalMapScene"); 
        }
    }

    public void OnNewGameButtonClicked()
    {
        if (nameInput.text != "")
        {
            gameParameters.gameName = nameInput.text;
            gameParameters.isNewGame = true;
            SceneManager.LoadScene("GlobalMapScene");
        }
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


    public void SelectGame(string name)
    {
        selectedGameName.text = name;
    }
}
