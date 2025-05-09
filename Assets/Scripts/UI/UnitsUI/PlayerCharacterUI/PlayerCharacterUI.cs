using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterUI : MonoBehaviour
{
    public static PlayerCharacterUI Instance;

    public GameObject UIContainer;
    public TMP_Text title;

    public ArmorCoreCellVisual armorCoreSlot;
    public ArtifactCellVisual artifactSlot;
    public WeaponCellVisual weapon1Slot, weapon2Slot;
    private WeaponCellVisual selectedWeaponCell;

    public TraitCellVisual traitCellPrefab;
    public SkillCellVisual skillCellPrefab;
    public Transform skills1Container, skills2Container, positiveTraitsContainer, negativeTraitsContainer;

    public TMP_Text healthLabel, defenceLabel, evasionLabel, attackLabel, accurancyLabel, critLabel, SPAmountLabel, SPRegenLabel, SPMoveCostLabel, speedLabel, tountLabel;

    public Image specialEnergy1, specialEnergy2;
    public TMP_Text specialEnergy1Amount, specialEnergy2Amount;
    
    public GameObject selectionPanel;
    public Transform selectionZoneContainer;
    public WeaponCellVisual weaponCellPrefab;
    public ArmorCoreCellVisual armorCoreCellPrefab;
    public ArtifactCellVisual artifactCellPrefab;

    private PlayerCharacter playerCharacter;
    private bool IsEditMode;

    void Awake()
    {
        Instance = this;
        enabled = false;
        UIContainer.SetActive(false);
        playerCharacter = null;
    }

    public void ShowForCharacter(PlayerCharacter character, bool isEditMode = false)
    {
        IsEditMode = isEditMode;
        playerCharacter = character;
        UpdateUI();
        enabled = true;
        UIContainer.SetActive(true);
        title.text = isEditMode ? " Character managment" : " Character info";
    }

    public void Hide()
    {
        selectionPanel.SetActive(false);
        enabled = false;
        UIContainer.SetActive(false);
        if(IsEditMode)
            MainBaseUI.Instance.enabled = true;
    }

    public void UpdateUI()
    {
        if(IsEditMode)
            MainBaseUI.Instance.enabled = false;
            
        selectionPanel.SetActive(false);
        selectedWeaponCell = null;
        HideSkillsInfo();
        HideTraits();
        HideSpecialEnergies();

        SetupSlots();
        UpdateInfo();
    }

    private void SetupSlots()
    {
        if (IsEditMode)
        {
            weapon1Slot.Setup(playerCharacter.equipment.weapon1, this, true);
            weapon2Slot.Setup(playerCharacter.equipment.weapon2, this, true);
            artifactSlot.Setup(playerCharacter.equipment.artifact, this);
            armorCoreSlot.Setup(playerCharacter.equipment.armorCore, this);
        }
        else
        {
            weapon1Slot.Setup(playerCharacter.equipment.weapon1, this, true);
            weapon2Slot.Setup(playerCharacter.equipment.weapon2, this, true);
            artifactSlot.Setup(playerCharacter.equipment.artifact);
            armorCoreSlot.Setup(playerCharacter.equipment.armorCore);
        }
    }

    private void UpdateInfo()
    {
        UpdateStats();
        ShowSkillsInfo();
        UpdateTraitsInfo();
    }

    private void UpdateStats()
    {
        healthLabel.text = $"{playerCharacter.currentCharacterStats.maxHealth}/{playerCharacter.currentCharacterStats.maxHealth}";
        defenceLabel.text = $"{playerCharacter.currentCharacterStats.defence}";
        evasionLabel.text = $"{playerCharacter.currentCharacterStats.evasion}";
        attackLabel.text = $"{playerCharacter.currentAttack1Stats.attack}/{playerCharacter.currentAttack2Stats.attack}";
        accurancyLabel.text = $"{playerCharacter.currentAttack1Stats.accuracy}/{playerCharacter.currentAttack2Stats.accuracy}";
        critLabel.text = $"{playerCharacter.currentAttack1Stats.critRate}/{playerCharacter.currentAttack2Stats.critRate}";
        SPAmountLabel.text = $"{playerCharacter.baseCharacterStats.SPamount}";
        SPRegenLabel.text = $"{playerCharacter.baseCharacterStats.SPregen}";
        SPMoveCostLabel.text = $"{playerCharacter.baseCharacterStats.SPmoveCost}";
        speedLabel.text = $"{playerCharacter.baseCharacterStats.speed}";
        tountLabel.text = $"{playerCharacter.baseCharacterStats.tount}";
    }

    private void ShowSkillsInfo()
    {
        HideSkillsInfo();

        for (int i = 0; i < playerCharacter.equipment.weapon1.skills.Count; i++)
        {
            SkillCellVisual skillCell = Instantiate(skillCellPrefab, skills1Container);
            skillCell.Setup(playerCharacter.equipment.weapon1.skills[i]);
        }
        specialEnergy1.gameObject.SetActive(true);
        specialEnergy1.color = SpecialEnergy.SpecialEnergyColors[0];
        specialEnergy1Amount.text = $"{playerCharacter.equipment.weapon1.specialEnergy.amount}";

        ClearPanel(skills2Container);

        for (int i = 0; i < playerCharacter.equipment.weapon2.skills.Count; i++)
        {
            SkillCellVisual skillCell = Instantiate(skillCellPrefab, skills2Container);
            skillCell.Setup(playerCharacter.equipment.weapon2.skills[i]);
        }

        specialEnergy2.gameObject.SetActive(true);
        specialEnergy2.color = SpecialEnergy.SpecialEnergyColors[0];
        specialEnergy2Amount.text = $"{playerCharacter.equipment.weapon2.specialEnergy.amount}";
    }

    private void HideSpecialEnergies()
    {
        specialEnergy1.gameObject.SetActive(false);
        specialEnergy2.gameObject.SetActive(false);
    }

    private void HideSkillsInfo()
    {
        ClearPanel(skills1Container);
        ClearPanel(skills2Container);
    }

    private void UpdateTraitsInfo()
    {
        HideTraits();

        List<Trait> positive = playerCharacter.GetTraits(TraitType.Positive);
        foreach (Trait trait in positive)
        {
            TraitCellVisual traitCell = Instantiate(traitCellPrefab, positiveTraitsContainer);
            traitCell.Setup(trait);
        }

        List<Trait> negative = playerCharacter.GetTraits(TraitType.Negatine);
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

    private void ShowSelectionPanel()
    {
        selectionPanel.SetActive(true);
        ClearPanel(selectionZoneContainer);
    }

    public void OnArmorCoreSlotSelection(ArmorCoreCellVisual armorCoreCell)
    {
        ShowSelectionPanel();
    }

    public void OnArmorCoreSwitch(ArmorCoreCellVisual armorCoreCell)
    {
        if (playerCharacter.equipment.armorCore != null)
            Base.Instance.inventory.Add(playerCharacter.equipment.armorCore);

        playerCharacter.equipment.armorCore = armorCoreCell.linkedArmor;
        Base.Instance.inventory.Remove(armorCoreCell.linkedArmor);
        UpdateUI();
    }

    public void OnWeaponSlotSelection(WeaponCellVisual weaponCell)
    {
        selectedWeaponCell = weaponCell;
        ShowSelectionPanel();

        foreach(Weapon weapon in playerCharacter.GetAvailableWeapons())
        {
            WeaponCellVisual newWeaponCell = Instantiate(weaponCellPrefab, selectionZoneContainer);
            newWeaponCell.Setup(weapon);
        }
    }

    public void OnWeaponSwitch(WeaponCellVisual weaponCell)
    {
        if (selectedWeaponCell == null)
            return;

        if (selectedWeaponCell == weapon1Slot)
        {
            playerCharacter.equipment.weapon1 = weaponCell.linkedWeapon;
        }
        else if (selectedWeaponCell == weapon2Slot)
        {
            playerCharacter.equipment.weapon2 = weaponCell.linkedWeapon;
        }
        UpdateUI();
    }

    public void OnArtifactSlotSelection(ArtifactCellVisual artifactSlot)
    {
        ShowSelectionPanel();

        Base.Instance.inventory.GetItems(out List<Artifact> artifacts);

        foreach(Artifact artifact in artifacts)
        {
            ArtifactCellVisual newArtifactCell = Instantiate(artifactCellPrefab, selectionZoneContainer);
            newArtifactCell.Setup(artifact, this);
        }
    }

    public void OnArtifactSwitch(ArtifactCellVisual artifactCell)
    {
        if (playerCharacter.equipment.artifact != null)
            Base.Instance.inventory.Add(playerCharacter.equipment.artifact);

        playerCharacter.equipment.artifact = artifactCell.linkedArtifact;
        Base.Instance.inventory.Remove(artifactCell.linkedArtifact);
        UpdateUI();
    }

    void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }
}
