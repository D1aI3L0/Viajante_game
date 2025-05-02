using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecruitingUI : MonoBehaviour
{
    public static RecruitingUI Instance;
    public GameObject recrutingPanel;

    public RectTransform recruitingCharactersContainer;
    public CharacterSlotBase characterSlotPrefab;

    public RectTransform weaponsContainer;
    public WeaponCellVisual weaponCellPrefab;
    public ArmorCoreCellVisual armorCoreCellPrefab;

    public RectTransform skillsContainer;
    public SkillCellVisual skillCellPrefab;

    public Button recruitButton;


    public TMP_Text healthLabel, defenceLabel, evasionLabel, attackLabel, accurancyLabel, critLabel, SPAmountLabel, SPRegenLabel, SPMoveCostLabel, initiativeLabel, agroLabel;

    private CharacterSlotBase selectedCharacterSlot;
    private WeaponCellVisual selectedWeapon;

    private List<PlayerCharacter> recruitingCharacters = new();
    public List<BasicCharacterTemplates> characterTemplates;

    public void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Hide()
    {
        recrutingPanel.SetActive(false);
    }

    public void Show()
    {
        recrutingPanel.SetActive(true);
        ShowCharacters();
    }

    private void ShowCharacters()
    {
        for (int i = 0; i < recruitingCharacters.Count; i++)
        {
            ClearPanel(recruitingCharactersContainer);

            List<PlayerCharacter> availableCharacters = new List<PlayerCharacter>();
            availableCharacters.AddRange(Base.Instance.availableCharacters);

            foreach (PlayerCharacter character in availableCharacters)
            {
                CharacterSlotBase slot = Instantiate(characterSlotPrefab, recruitingCharactersContainer);
                slot.Initialize(character, this);
            }
        }
    }

    public void OnCharacterSelection(CharacterSlotBase characterSlotBase)
    {
        if (selectedCharacterSlot)
        {
            selectedCharacterSlot.ToggleButtonsVisibility(true);
            selectedCharacterSlot.highlight.SetActive(false);
        }

        selectedCharacterSlot = characterSlotBase;

        ShowCharacterStats();
        ShowCharacterEquipment();
    }

    public void OnCharacterUnselect()
    {
        HideCharacterStats();
        HideCharacterEquipment();
        HideSkillset();
    }

    private void ShowCharacterStats()
    {
        if (selectedCharacterSlot.linkedCharacter == null)
            return;
    }

    private void HideCharacterStats()
    {
        healthLabel.text = defenceLabel.text = evasionLabel.text =
        attackLabel.text = accurancyLabel.text = critLabel.text =
        SPAmountLabel.text = SPRegenLabel.text = SPMoveCostLabel.text =
        initiativeLabel.text = agroLabel.text = "---";
    }

    private void ShowCharacterEquipment()
    {
        if (selectedCharacterSlot.linkedCharacter == null)
            return;

        var weapons = selectedCharacterSlot.linkedCharacter.GetAvailableWeapons();
        foreach (Weapon weapon in weapons)
        {
            WeaponCellVisual slot = Instantiate(weaponCellPrefab, weaponsContainer);
            slot.Setup(weapon, this);
        }
    }

    private void HideCharacterEquipment()
    {
        ClearPanel(weaponsContainer);
    }

    public void OnWeaponSelection(WeaponCellVisual weaponCellVisual)
    {
        ShowSkillset(weaponCellVisual.linkedWeapon);
    }

    private void ShowSkillset(Weapon weapon)
    {
        if (weapon == null)
            return;

        foreach (Skill skill in weapon.skills)
        {
            SkillCellVisual slot = Instantiate(skillCellPrefab, skillsContainer);
            slot.Setup(skill);
        }
    }

    private void HideSkillset()
    {
        ClearPanel(skillsContainer);
    }

    public void UpdateRecruitingCharacters()
    {
        recruitingCharacters.Clear();
        ClearPanel(recruitingCharactersContainer);
    }

    void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }
}