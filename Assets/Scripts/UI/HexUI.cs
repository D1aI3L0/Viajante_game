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
            UIReferences.hexMapEditor.Toggle(!UIReferences.hexMapEditor.enabled);
            GameUI.Instance.Toggle(!UIReferences.hexMapEditor.enabled);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            UIReferences.hexMapEditor.ShowGrid(!UIReferences.hexMapEditor.gridIsVisible);
        }
    }


    public void DisableAllUnitsUI()
    {
        MainBaseUI.Instance.Hide();
        UIReferences.playerSquadUI.Hide();
        UIReferences.playerCharacterUI.Hide();
    }
}
