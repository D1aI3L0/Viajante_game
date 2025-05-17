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
    public Button recruitButton;

    public RectTransform weaponsContainer;
    public WeaponCellVisual weaponCellPrefab;

    public RectTransform skillsContainer;
    public SkillCellVisual skillCellPrefab;

    public RectTransform positiveTraitsContainer, negativeTraitsContainer;
    public TraitCellVisual traitCellPrefab;

    public TMP_Text healthLabel, defenceLabel, evasionLabel, attackLabel, accurancyLabel, critLabel, SPAmountLabel, SPRegenLabel, SPMoveCostLabel, speedLabel, tountLabel;

    private CharacterSlotBase selectedCharacterSlot;
    private WeaponCellVisual selectedWeaponCell;

    public RecruitingController recruitingController;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Show()
    {
        enabled = true;
        recrutingPanel.SetActive(true);
        HideCharacterEquipment();
        HideCharacterStats();
        HideSkillset();
        HideTraits();
        ShowCharacters();
    }

    public void Hide()
    {
        GameUI.Instance.enabled = true;
        enabled = false;
        recrutingPanel.SetActive(false);
        OnCharacterUnselect();
        ClearPanel(recruitingCharactersContainer);
    }

    private void ShowCharacters()
    {
        ClearPanel(recruitingCharactersContainer);
        List<PlayerCharacter> availableCharacters = recruitingController.GetCharacters();

        foreach (PlayerCharacter character in availableCharacters)
        {
            CharacterSlotBase slot = Instantiate(characterSlotPrefab, recruitingCharactersContainer);
            slot.Initialize(character, this);
        }
    }

    public void UpdateCharactersPanel()
    {
        OnCharacterUnselect();
        ShowCharacters();
    }

    public void OnCharacterSelection(CharacterSlotBase characterSlotBase)
    {
        if (selectedCharacterSlot)
        {
            selectedCharacterSlot.ToggleButtonsVisibility(true);
            selectedCharacterSlot.highlight.SetActive(false);
        }

        selectedCharacterSlot = characterSlotBase;

        recruitButton.interactable = true;
        ShowCharacterStats();
        ShowCharacterEquipment();
        ShowCharacterTraits();
    }

    public void OnCharacterUnselect()
    {
        recruitButton.interactable = false;
        selectedCharacterSlot = null;
        HideCharacterStats();
        HideCharacterEquipment();
        HideSkillset();
        HideCharacterStats();
    }

    private void ShowCharacterStats()
    {
        if (selectedCharacterSlot.linkedCharacter == null)
            return;

        PlayerCharacter selectedCharacter = selectedCharacterSlot.linkedCharacter;

        healthLabel.text = $"{selectedCharacter.baseCharacterStats.maxHealth}";
        defenceLabel.text = $"{selectedCharacter.baseCharacterStats.defence}";
        evasionLabel.text = $"{selectedCharacter.baseCharacterStats.evasion}";
        speedLabel.text = $"{selectedCharacter.baseCharacterStats.speed}";
        tountLabel.text = $"{selectedCharacter.baseCharacterStats.tount}";
        SPAmountLabel.text = $"{selectedCharacter.baseCharacterStats.SPamount}";
        SPRegenLabel.text = $"{selectedCharacter.baseCharacterStats.SPregen}";
        SPMoveCostLabel.text = $"{selectedCharacter.baseCharacterStats.SPmoveCost}";
    }

    private void ShowWeaponStats(Weapon weapon)
    {
        attackLabel.text = $"{weapon.weaponParameters.ATK}";
        accurancyLabel.text = $"{weapon.weaponParameters.ACC}";
        critLabel.text = $"{weapon.weaponParameters.CRIT}";
    }

    private void HideCharacterStats()
    {
        healthLabel.text = defenceLabel.text = evasionLabel.text =
        attackLabel.text = accurancyLabel.text = critLabel.text =
        SPAmountLabel.text = SPRegenLabel.text = SPMoveCostLabel.text =
        speedLabel.text = tountLabel.text = "---";
    }

    private void ShowCharacterEquipment()
    {
        if (selectedCharacterSlot.linkedCharacter == null)
            return;

        HideCharacterEquipment();

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
        selectedWeaponCell = weaponCellVisual;
        ShowWeaponStats(weaponCellVisual.linkedWeapon);
        ShowSkillset(weaponCellVisual.linkedWeapon);
    }

    private void ShowSkillset(Weapon weapon)
    {
        if (weapon == null)
            return;

        HideSkillset();

        foreach (SkillAsset skill in weapon.skillSet.skills)
        {
            SkillCellVisual slot = Instantiate(skillCellPrefab, skillsContainer);
            slot.Setup(skill);
        }
    }

    private void HideSkillset()
    {
        ClearPanel(skillsContainer);
    }

    private void ShowCharacterTraits()
    {
        HideTraits();

        List<Trait> positive = selectedCharacterSlot.linkedCharacter.GetTraits(TraitType.Positive);
        foreach (Trait trait in positive)
        {
            TraitCellVisual traitCell = Instantiate(traitCellPrefab, positiveTraitsContainer);
            traitCell.Setup(trait);
        }

        List<Trait> negative = selectedCharacterSlot.linkedCharacter.GetTraits(TraitType.Negatine);
        foreach (Trait trait in negative)
        {
            TraitCellVisual traitCell = Instantiate(traitCellPrefab, negativeTraitsContainer);
            traitCell.Setup(trait);
        }
    }

    private void HideTraits()
    {
        ClearPanel(positiveTraitsContainer);
        ClearPanel(negativeTraitsContainer);
    }

    public void OnRecruitButtonClick()
    {
        if (selectedCharacterSlot.linkedCharacter != null)
        {
            Base.Instance.AddCharacter(selectedCharacterSlot.linkedCharacter);
            recruitingController.Remove(selectedCharacterSlot.linkedCharacter);
            Show();
        }
    }

    private void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }
}