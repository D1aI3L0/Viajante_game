using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class SkillCellVisual : CellVisual, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private SkillAsset linkedSkill;
    private delegate void OnClick();
    private OnClick onClick = () => { };

    public void Setup(SkillAsset skill, EquipmentUpgradeUI equipmentUpgradeUI)
    {
        linkedSkill = skill;
        onClick = () => equipmentUpgradeUI.OnSkillSelection(linkedSkill);
    }

    public void Setup(SkillAsset skill)
    {
        linkedSkill = skill;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        onClick();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

    }

    public void OnPointerExit(PointerEventData eventData)
    {

    }
}