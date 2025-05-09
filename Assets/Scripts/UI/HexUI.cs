using Unity.VisualScripting;
using UnityEngine;

public class HexUI : MonoBehaviour
{
    public static HexUI Instance;
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            HexMapEditor.Instance.Toggle(!HexMapEditor.Instance.enabled);
            GameUI.Instance.Toggle(!HexMapEditor.Instance.enabled);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            HexMapEditor.Instance.ShowGrid(!HexMapEditor.Instance.gridIsVisible);
        }
    }


    public void DisableAllUnitsUI()
    {
        MainBaseUI.Instance.Hide();
        PlayerCharacterUI.Instance.Hide();
        PlayerSquadUI.Instance.Hide();
    }
}
