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

    public List<TraitData> availablePositiveTraits, availableNegativeTraits;
    public RectTransform positiveTraitsContainer, negativeTraitsContainer;
    public TraitCellVisual traitCellPrefab;


    public TMP_Text healthLabel, defenceLabel, evasionLabel, attackLabel, accurancyLabel, critLabel, SPAmountLabel, SPRegenLabel, SPMoveCostLabel, initiativeLabel, tountLabel;

    private CharacterSlotBase selectedCharacterSlot;
    private WeaponCellVisual selectedWeaponCell;

    private List<PlayerCharacter> recruitingCharacters = new();
    public List<BasicCharacterTemplates> characterTemplates;

    private const int recruitmentCharactersPerCycleCount = 4;

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
        ShowCharacters();
    }

    public void Hide()
    {
        enabled = false;
        recrutingPanel.SetActive(false);
    }

    private void ShowCharacters()
    {
        for (int i = 0; i < recruitingCharacters.Count; i++)
        {
            ClearPanel(recruitingCharactersContainer);

            foreach (PlayerCharacter character in recruitingCharacters)
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

        recruitButton.interactable = true;
        ShowCharacterStats();
        ShowCharacterEquipment();
    }

    public void OnCharacterUnselect()
    {
        recruitButton.interactable = false;
        selectedCharacterSlot = null;
        HideCharacterStats();
        HideCharacterEquipment();
        HideSkillset();
    }

    private void ShowCharacterStats()
    {
        if (selectedCharacterSlot.linkedCharacter == null)
            return;

        PlayerCharacter selectedCharacter = selectedCharacterSlot.linkedCharacter;

        healthLabel.text = $"{selectedCharacter.baseCharacterStats.maxHealth}";
        defenceLabel.text = $"{selectedCharacter.baseCharacterStats.defence}";
        evasionLabel.text = $"{selectedCharacter.baseCharacterStats.evasion}";
        initiativeLabel.text = $"{selectedCharacter.baseCharacterStats.speed}";
        tountLabel.text = $"{selectedCharacter.baseCharacterStats.tount}";
        SPAmountLabel.text = $"{selectedCharacter.baseCharacterStats.SPamount}";
        SPRegenLabel.text = $"{selectedCharacter.baseCharacterStats.SPregen}";
        SPMoveCostLabel.text = $"{selectedCharacter.baseCharacterStats.SPmoveCost}";
    }

    private void ShowWeaponStats(Weapon weapon)
    {
        attackLabel.text = $"{weapon.attackStats.attack}";
        accurancyLabel.text = $"{weapon.attackStats.accuracy}";
        critLabel.text = $"{weapon.attackStats.critRate}";
    }

    private void HideCharacterStats()
    {
        healthLabel.text = defenceLabel.text = evasionLabel.text =
        attackLabel.text = accurancyLabel.text = critLabel.text =
        SPAmountLabel.text = SPRegenLabel.text = SPMoveCostLabel.text =
        initiativeLabel.text = tountLabel.text = "---";
    }

    private void ShowCharacterEquipment()
    {
        if (selectedCharacterSlot.linkedCharacter == null)
            return;

        ClearPanel(weaponsContainer);

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

        if (characterTemplates.Count > 0)
        {
            for (int i = 0; i < recruitmentCharactersPerCycleCount; i++)
            {
                PlayerCharacter newCharacter = new();
                newCharacter.Initialize(characterTemplates[UnityEngine.Random.Range(0, characterTemplates.Count)]);
                recruitingCharacters.Add(newCharacter);
            }
        }
    }

    public void OnRecruitButtonClick()
    {
        if (selectedCharacterSlot.linkedCharacter != null)
        {
            Base.Instance.AddCharacter(selectedCharacterSlot.linkedCharacter);
            recruitingCharacters.Remove(selectedCharacterSlot.linkedCharacter);
            Show();
        }
    }

    void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }
}