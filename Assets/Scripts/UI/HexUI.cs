using Unity.VisualScripting;
using UnityEngine;

public class HexUI : MonoBehaviour
{
    void Awake()
    {
        UIReferences.hexUI = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            UIReferences.hexMapEditor.Toggle(!UIReferences.hexMapEditor.enabled);
            UIReferences.gameUI.Toggle(!UIReferences.hexMapEditor.enabled);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            UIReferences.hexMapEditor.ShowGrid(!UIReferences.hexMapEditor.gridIsVisible);
        }
    }


    public void DisableAllUnitsUI()
    {
        UIReferences.mainBaseUI.Hide();
        UIReferences.playerSquadUI.Hide();
        UIReferences.playerCharacterUI.Hide();
    }
}
