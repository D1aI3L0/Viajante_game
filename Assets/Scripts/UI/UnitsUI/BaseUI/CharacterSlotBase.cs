using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSlotBase : MonoBehaviour
{
    public PlayerCharacter linkedCharacter;
    public Image characterIcon;
    public TMP_Text characterName;
    public Button selectButton;
    public Button unselectButton;
    public GameObject highlight;

    public void Initialize(PlayerCharacter character, SquadCreationUI squadCreationUI, bool isInSquadPanel)
    {
        linkedCharacter = character;
        characterName.text = character.characterName;
        
        selectButton.gameObject.SetActive(!isInSquadPanel);
        unselectButton.gameObject.SetActive(isInSquadPanel);

        if (!isInSquadPanel)
        {
            selectButton.onClick.AddListener(() => squadCreationUI.ToggleCharacterSelection(linkedCharacter, true));
        }
        else
        {
            unselectButton.onClick.AddListener(() => squadCreationUI.ToggleCharacterSelection(linkedCharacter, false));
        }
    }

    public void Initialize(PlayerCharacter character, EquipmentUpgradeUI equipmentUpgradeUI)
    {
        linkedCharacter = character;
        characterName.text = character.characterName;

        selectButton.onClick.AddListener(() => {highlight.SetActive(true); ToggleButtonsVisibility(false); equipmentUpgradeUI.OnCharacterSelection(this);});
        unselectButton.onClick.AddListener(() => {highlight.SetActive(false); ToggleButtonsVisibility(true); equipmentUpgradeUI.OnCharacterUnselect();});
    }

    public void Initialize(PlayerCharacter character, RecruitingUI recruitingUI)
    {
        linkedCharacter = character;
        characterName.text = character.characterName;

        selectButton.onClick.AddListener(() => {highlight.SetActive(true); ToggleButtonsVisibility(false); recruitingUI.OnCharacterSelection(this);});
        unselectButton.onClick.AddListener(() => {highlight.SetActive(false); ToggleButtonsVisibility(true); recruitingUI.OnCharacterUnselect();});
    }

    public void ToggleButtonsVisibility(bool toggle)
    {
        selectButton.gameObject.SetActive(toggle);
        unselectButton.gameObject.SetActive(!toggle);
    }
}