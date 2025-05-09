using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class CharacterSlotBase : MonoBehaviour, IPointerClickHandler
{
    public PlayerCharacter linkedCharacter;
    public Image characterIcon;
    public TMP_Text characterName;
    public Button selectButton;
    public Button unselectButton;
    public Button infoButton;
    public GameObject highlight;
    private bool OnClickActive = false;

    public void Initialize(PlayerCharacter character, SquadCreationUI squadCreationUI, bool isInSquadPanel)
    {
        OnClickActive = true;
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

        infoButton.gameObject.SetActive(true);
        infoButton.onClick.AddListener(() => PlayerCharacterUI.Instance.ShowForCharacter(linkedCharacter, true));
    }

    public void Initialize(PlayerCharacter character, EquipmentUpgradeUI equipmentUpgradeUI)
    {
        OnClickActive = false;
        linkedCharacter = character;
        characterName.text = character.characterName;

        selectButton.onClick.AddListener(() => { highlight.SetActive(true); ToggleButtonsVisibility(false); equipmentUpgradeUI.OnCharacterSelection(this); });
        unselectButton.onClick.AddListener(() => { highlight.SetActive(false); ToggleButtonsVisibility(true); equipmentUpgradeUI.OnCharacterUnselect(); });
    }

    public void Initialize(PlayerCharacter character, RecruitingUI recruitingUI)
    {
        OnClickActive = false;
        linkedCharacter = character;
        characterName.text = character.characterName;

        selectButton.onClick.AddListener(() => { highlight.SetActive(true); ToggleButtonsVisibility(false); recruitingUI.OnCharacterSelection(this); });
        unselectButton.onClick.AddListener(() => { highlight.SetActive(false); ToggleButtonsVisibility(true); recruitingUI.OnCharacterUnselect(); });
    }

    public void ToggleButtonsVisibility(bool toggle)
    {
        selectButton.gameObject.SetActive(toggle);
        unselectButton.gameObject.SetActive(!toggle);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(OnClickActive)
        {
            PlayerCharacterUI.Instance.ShowForCharacter(linkedCharacter, true);
        }
    }
}