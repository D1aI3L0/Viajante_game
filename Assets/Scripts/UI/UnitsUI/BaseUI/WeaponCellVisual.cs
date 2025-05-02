using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponCellVisual : CellVisual, IPointerClickHandler
{
    public Weapon linkedWeapon;
    private delegate void OnButtonClick();
    private OnButtonClick onButtonClick;

    public void Setup(Weapon weapon, RecruitingUI recruitingUI)
    {
        linkedWeapon = weapon;
        onButtonClick = () => { recruitingUI.OnWeaponSelection(this); };
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onButtonClick();
    }
}
