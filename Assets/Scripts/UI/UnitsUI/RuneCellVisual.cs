using UnityEngine;
using UnityEngine.EventSystems;

public class RuneCellVisual : CellVisual, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Rune linkedRune;

    public GameObject[] runeConnectionsVisual;

    public void Setup(Rune rune)
    {
        linkedRune = rune;
        foreach (HexDirection d in linkedRune.runeConnections)
        {
            runeConnectionsVisual[(int)d].SetActive(true);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EquipmentUpgradeUI.Instance.OnRuneSelection(linkedRune);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }
}