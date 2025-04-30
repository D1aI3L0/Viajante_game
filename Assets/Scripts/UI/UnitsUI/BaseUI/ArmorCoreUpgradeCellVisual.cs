using TMPro;
using UnityEngine;

public class ArmorCoreUpgradeCellVisual : UpgradeCellVisual
{
    public TMP_Text bonusText;
    
    [Header("Цвета")]
    public Color healthColor = Color.red;
    public Color defenceColor = Color.blue;
    public Color evasionColor = Color.green;
    public Color defaultColor = Color.white;
    public Color burnedColor = Color.gray;
    
    public void Setup(Upgrade upgrade)
    {   
        if(upgrade is not ArmorCoreUpgrade)
            return;

        ArmorCoreUpgrade armorCoreUpgrade = (ArmorCoreUpgrade)upgrade;
        
        if (armorCoreUpgrade.isBurned)
        {
            icon.color = burnedColor;
            bonusText.text = "---";
            return;
        }
        
        switch (armorCoreUpgrade.statType)
        {
            case SurvivalStatType.Health:
                icon.color = healthColor;
                break;
            case SurvivalStatType.Defence:
                icon.color = defenceColor;
                break;
            case SurvivalStatType.Evasion:
                icon.color = evasionColor;
                break;
            default:
                icon.color = defaultColor;
                break;
        }
        
        bonusText.text = $"+{armorCoreUpgrade.bonusValue}%";
    }
}