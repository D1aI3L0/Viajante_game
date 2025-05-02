using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSlotSquad : MonoBehaviour
{
    [Header("Элементы")]
    public Image characterIcon;
    public Image healthIndicator;
    public TMP_Text nameText;
    
    private PlayerCharacter character;
    
    public void Initialize(PlayerCharacter character)
    {
        this.character = character;
        nameText.text = character.characterName;
        
        UpdateStats();
    }
    
    public void UpdateStats()
    {
        healthIndicator.fillAmount = (float)character.currentSurvivalStats.health / character.maxSurvivalStats.health;
    }

    public void ShowCharacterStats()
    {
        if(character is PlayerCharacter playerCharacter)
            UIReferences.playerCharacterUI.ShowForCharacter(playerCharacter);
    }
}