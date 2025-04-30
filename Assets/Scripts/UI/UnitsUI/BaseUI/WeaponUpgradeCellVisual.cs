using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WeaponUpgradeCellVisual : UpgradeCellVisual, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject skillHightight;
    public GameObject[] runeConnectionsVisual;

    private WeaponUpgrade linkedUpgrade;

    private static readonly List<Color> skillCellColors = new()
    {
        Color.yellow,
        Color.cyan,
        Color.magenta,
        Color.green
    };

    public bool isSelected = false;
    private Color cellColor;
    private Color highlightColor = Color.red;

    public void Setup(Upgrade upgrade, int colorNumber)
    {
        if (upgrade is not WeaponUpgrade)
            return;

        linkedUpgrade = (WeaponUpgrade)upgrade;

        cellColor = skillCellColors[colorNumber];
        icon.color = cellColor;

        if (upgrade is WeaponUpgradeSkill)
            skillHightight.SetActive(true);

        if (upgrade is WeaponUpgradeRune upgradeRune && upgradeRune.linkedRune != null)
        {
            foreach (HexDirection d in upgradeRune.linkedRune.runeConnections)
            {
                runeConnectionsVisual[(int)d].SetActive(true);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        icon.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
            icon.color = cellColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isSelected)
        {
            isSelected = true;
            EquipmentUpgradeUI.Instance.OnUpgradeSelection(this);
            OnPointerEnter(null);
        }
    }

    public WeaponUpgrade GetLinkedUpgrade()
    {
        return linkedUpgrade;
    }

    public void Unselect()
    {
        isSelected = false;
        OnPointerExit(null);
    }
}