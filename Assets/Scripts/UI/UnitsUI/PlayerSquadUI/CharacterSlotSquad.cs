using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CharacterSlotSquad : MonoBehaviour
{
    [Header("Элементы")]
    public Image characterIcon;
    public Slider healthSlider;
    public Slider staminaSlider;
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
        healthSlider.value = (float)character.currentCharactetStats.health / character.maxCharacterStats.health;
        staminaSlider.value = (float)character.Stamina / character.maxStamina;
    }
}