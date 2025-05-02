using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ArmorCoreCellVisual : CellVisual, IPointerClickHandler
{
    private ArmorCore linkedArmor;
    private delegate void OnButtonClick();
    private OnButtonClick onButtonClick;

    public void Setup(ArmorCore armorCore, RecruitingUI recruitingUI)
    {
        linkedArmor = armorCore;
        //onButtonClick = () => { recruitingUI.OnArmorCoreSelection(linkedArmor); };
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onButtonClick();
    }
}
