using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HexUI : MonoBehaviour
{
    public static HexUI Instance;
    [SerializeField] private GameObject UIContainer;

    private bool PauseEnabled { get; set; }

    void Awake()
    {
        Instance = this;
        TogglePause(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) && !PauseEnabled)
        {
            HexMapEditor.Instance.Toggle(!HexMapEditor.Instance.enabled);
            GameUI.Instance.Toggle(!HexMapEditor.Instance.enabled);
            HideAllUIs();
        }
        if (Input.GetKeyDown(KeyCode.F2) && !PauseEnabled)
        {
            HexMapEditor.Instance.ShowGrid(!HexMapEditor.Instance.gridIsVisible);
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause(!UIContainer.activeSelf);
        }
    }

    public void HideAllUIs()
    {
        MainBaseUI.Instance.Hide();
        PlayerCharacterUI.Instance.Hide();
        PlayerSquadUI.Instance.Hide();
    }

    private void TogglePause(bool toggle)
    {
        HexMapCamera.Locked = toggle;
        PauseEnabled = toggle;
        UIContainer.SetActive(toggle);
    }

    public void OnSaveAndQuitClick()
    {
        SaveLoadMenu.Instance.SaveGame(GlobalMapGameManager.Instance.GetGameName());
        SceneManager.LoadScene("MainMenu");
    }

    public void OnSaveClick()
    {
        SaveLoadMenu.Instance.SaveGame(GlobalMapGameManager.Instance.GetGameName());
        TogglePause(!UIContainer.activeSelf);
    }

    public void OnOptionsClick()
    {

    }
}
