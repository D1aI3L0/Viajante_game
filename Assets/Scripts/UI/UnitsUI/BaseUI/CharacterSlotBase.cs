using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSlot : MonoBehaviour
{
    public Character character;
    public Image characterIcon;
    public TMP_Text characterName;
    public Button selectButton;
    public Button removeButton;

    public void Initialize(Character character, SquadCreationUI squadCreationUI, bool isInSquadPanel)
    {
        this.character = character;
        characterName.text = character.characterName;
        
        selectButton.gameObject.SetActive(!isInSquadPanel);
        removeButton.gameObject.SetActive(isInSquadPanel);

        if (!isInSquadPanel)
        {
            selectButton.onClick.AddListener(() => squadCreationUI.ToggleCharacterSelection(character, true));
        }
        else
        {
            removeButton.onClick.AddListener(() => squadCreationUI.ToggleCharacterSelection(character, false));
        }
    }
}