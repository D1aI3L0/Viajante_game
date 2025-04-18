// ArmorCoreHexCellVisual.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArmorCoreHexCellVisual : MonoBehaviour
{
    [Header("Визуальные элементы")]
    public Image background;
    public Image icon;
    public TMP_Text bonusText;
    public GameObject burnedOverlay;
    public GameObject availableOverlay;
    
    [Header("Цвета")]
    public Color healthColor = Color.red;
    public Color defenceColor = Color.blue;
    public Color evasionColor = Color.green;
    public Color defaultColor = Color.white;
    public Color burnedColor = Color.gray;
    
    public void Setup(ArmorCoreUpgrade upgrade, bool isAvailable = false)
    {
        burnedOverlay.SetActive(upgrade.isBurned);
        availableOverlay.SetActive(isAvailable);
        
        if (upgrade.isBurned)
        {
            background.color = burnedColor;
            bonusText.text = "X";
            icon.gameObject.SetActive(false);
            return;
        }
        
        switch (upgrade.statType)
        {
            case SurvivalStatType.Health:
                background.color = healthColor;
                icon.color = healthColor;
                break;
            case SurvivalStatType.Defence:
                background.color = defenceColor;
                icon.color = defenceColor;
                break;
            case SurvivalStatType.Evasion:
                background.color = evasionColor;
                icon.color = evasionColor;
                break;
            default:
                background.color = defaultColor;
                icon.color = defaultColor;
                break;
        }
        
        bonusText.text = $"+{upgrade.bonusValue}%";
        icon.gameObject.SetActive(true);
    }
}