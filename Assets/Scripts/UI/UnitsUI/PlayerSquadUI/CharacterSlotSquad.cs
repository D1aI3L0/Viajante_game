using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSlotSquad : MonoBehaviour
{
    [Header("Элементы")]
    public Image characterIcon;
    public Slider healthSlider;
    public TMP_Text nameText;
    
    private Character character;
    
    public void Initialize(Character character)
    {
        this.character = character;
        nameText.text = character.characterName;
        
        UpdateStats();
    }
    
    public void UpdateStats()
    {
        healthSlider.value = (float)character.currentSurvivalStats.health / character.maxSurvivalStats.health;
    }

    public void ShowCharacterStats()
    {
        if(character is PlayerCharacter playerCharacter)
            UIReferences.playerCharacterUI.ShowForCharacter(playerCharacter);
    }
}