using UnityEngine;

using System.IO;
using UnityEngine.UIElements;

public class HexUI : MonoBehaviour
{
    public HexMapEditor hexMapEditor;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            hexMapEditor.Toggle();
        }
    }
}
