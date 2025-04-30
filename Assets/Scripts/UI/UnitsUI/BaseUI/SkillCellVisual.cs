using UnityEngine;
using UnityEngine.EventSystems;

public class SkillCellVisual : CellVisual, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private Skill linkedSkill;
    
    public void Setup(Skill skill)
    {
        linkedSkill = skill;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EquipmentUpgradeUI.Instance.OnSkillSelection(linkedSkill);
    }

    virtual public void OnPointerEnter(PointerEventData eventData)
    {
        
    }

    virtual public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}