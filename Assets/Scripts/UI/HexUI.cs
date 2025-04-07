using Unity.VisualScripting;
using UnityEngine;

public class HexUI : MonoBehaviour
{
    public HexMapEditor hexMapEditor;
    public GameUI gameUI;
    public MainBaseUI mainBaseUI;
    public PlayerSquadUI playerSquadUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            hexMapEditor.Toggle(!hexMapEditor.enabled);
            gameUI.ToggleEditMode(!hexMapEditor.enabled);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            hexMapEditor.ShowGrid(!hexMapEditor.gridIsVisible);
        }
    }


    public void DisableAllUnitsUI()
    {
        mainBaseUI.HideMenu();
        mainBaseUI.enabled = false;
        playerSquadUI.Hide();
    }
}
