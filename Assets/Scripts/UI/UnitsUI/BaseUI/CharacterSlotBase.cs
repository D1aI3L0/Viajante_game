using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSlotBase : MonoBehaviour
{
    public Character character;
    public Image characterIcon;
    public TMP_Text characterName;
    public Button selectButton;
    public Button unselectButton;
    public GameObject highlight;

    public void Initialize(PlayerCharacter character, SquadCreationUI squadCreationUI, bool isInSquadPanel)
    {
        this.character = character;
        characterName.text = character.characterName;
        
        selectButton.gameObject.SetActive(!isInSquadPanel);
        unselectButton.gameObject.SetActive(isInSquadPanel);

        if (!isInSquadPanel)
        {
            selectButton.onClick.AddListener(() => squadCreationUI.ToggleCharacterSelection(character, true));
        }
        else
        {
            unselectButton.onClick.AddListener(() => squadCreationUI.ToggleCharacterSelection(character, false));
        }
    }

    public void Initialize(PlayerCharacter character, EquipmentUpgradeUI equipmentUpgradeUI)
    {
        this.character = character;
        characterName.text = character.characterName;

        selectButton.onClick.AddListener(() => {highlight.SetActive(true); ToggleButtonsVisibility(false); equipmentUpgradeUI.SelectCharacter(this, character);});
        unselectButton.onClick.AddListener(() => {highlight.SetActive(false); ToggleButtonsVisibility(true); equipmentUpgradeUI.UnselectCharacter();});
    }

    public void Initialize(PlayerCharacter character, RecruitingUI recruitingUI)
    {
        
    }

    public void ToggleButtonsVisibility(bool toggle)
    {
        selectButton.gameObject.SetActive(toggle);
        unselectButton.gameObject.SetActive(!toggle);
    }
}