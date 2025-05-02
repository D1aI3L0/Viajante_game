using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillCellVisual : CellVisual, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Skill linkedSkill;
    private delegate void OnClick();
    private OnClick onClick = () => { };

    public void Setup(Skill skill, EquipmentUpgradeUI equipmentUpgradeUI)
    {
        linkedSkill = skill;
        onClick = () => equipmentUpgradeUI.OnSkillSelection(linkedSkill);
    }

    public void Setup(Skill skill)
    {
        linkedSkill = skill;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick();
    }

    virtual public void OnPointerEnter(PointerEventData eventData)
    {

    }

    virtual public void OnPointerExit(PointerEventData eventData)
    {

    }
}