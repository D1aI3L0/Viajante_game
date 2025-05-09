using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArmorCoreCellVisual : CellVisual, IPointerClickHandler
{
    public ArmorCore linkedArmor;
    private delegate void OnButtonClick();
    private OnButtonClick onButtonClick;

    public void Setup(ArmorCore armorCore)
    {
        linkedArmor = armorCore;
    }

    public void Setup(ArmorCore armorCore, PlayerCharacterUI playerCharacterUI, bool isSlot = false)
    {
        linkedArmor = armorCore;
        onButtonClick = () => 
        { 
            if(isSlot) playerCharacterUI.OnArmorCoreSwitch(this);
            else playerCharacterUI.OnArmorCoreSlotSelection(this);
        };
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onButtonClick();
    }
}
