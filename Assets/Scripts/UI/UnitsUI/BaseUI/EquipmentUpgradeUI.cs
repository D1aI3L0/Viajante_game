using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;
using System.Linq;

public class EquipmentUpgradeUI : MonoBehaviour
{
    public static EquipmentUpgradeUI Instance;
    public GameObject UIContainer;
    public RectTransform charactersContainer;
    public CharacterSlotBase characterSlotPrefab;
    public Button armoreCoreButton, weapon1Button, weapon2Button;
    public Button upgradeButton, resetUpgradesButton;
    public TMP_Text itemInfo;

    public GameObject selectionPanel;
    public RectTransform selectionContainer;
    public RuneCellVisual runeCellPrefab;
    public SkillCellVisual skillCellPrefab;

    private CharacterSlotBase selectedCharacterSlot;
    private PlayerCharacter selectedCharacter;
    private Item selectedItem;

    [Header("Сетка прокачки")]
    public GameObject upgradeGridPanel;
    public RectTransform upgradeGridContainer;
    public ArmorCoreUpgradeCellVisual armorCoreUpgradeCellPrefab;
    public WeaponUpgradeCellVisual weaponUpgradeCellPrefab;
    public UpgradeCellVisual lockedUpgradeCellPrefab;

    private const float cellOuterToInner = 0.866025404f;
    private const float cellInnerRadius = cellOuterRadius * cellOuterToInner;
    private const float cellOuterRadius = 30f;
    private Dictionary<Vector2Int, UpgradeCellVisual> upgradeCells = new();
    private Vector2 upgradeGridContainerCenter;

    private UpgradeCellVisual selectedUpgradeCell;

    private List<CellVisual> selectionCells = new();

    public void Awake()
    {
        Instance = this;
        upgradeGridContainerCenter = new Vector2(upgradeGridContainer.rect.center.x, upgradeGridContainer.rect.center.y);
        HideSelectionPanel();
    }

    public void Show()
    {
        upgradeButton.gameObject.SetActive(false);
        enabled = true;
        UIContainer.SetActive(true);
        ShowhCharacters();
    }

    public void Hide()
    {
        enabled = false;
        UIContainer.SetActive(false);
    }

    private void ShowhCharacters()
    {
        ClearPanel(charactersContainer);

        List<PlayerCharacter> availableCharacters = new List<PlayerCharacter>();
        availableCharacters.AddRange(Base.Instance.availableCharacters);

        foreach (PlayerCharacter character in availableCharacters)
        {
            CharacterSlotBase slot = Instantiate(characterSlotPrefab, charactersContainer);
            slot.Initialize(character, this);
        }
    }

    public void OnCharacterSelection(CharacterSlotBase characterSlotBase)
    {
        if (selectedCharacterSlot)
        {
            selectedCharacterSlot.ToggleButtonsVisibility(true);
            selectedCharacterSlot.highlight.SetActive(false);
        }

        selectedCharacter = characterSlotBase.linkedCharacter;
        selectedCharacterSlot = characterSlotBase;

        HideSelectionPanel();
        ClearHexGrid();
        ShowCharacterEquipment();
    }

    public void OnCharacterUnselect()
    {
        selectedCharacter = null;
        HideSelectionPanel();
        ClearHexGrid();
        HideCharacterEquipment();
        resetUpgradesButton.gameObject.SetActive(false);
    }

    void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }

    private void ShowCharacterEquipment()
    {
        if (selectedCharacter == null)
            return;

        if (selectedCharacter.equipment.armorCore != null)
        {
            armoreCoreButton.interactable = true;
            armoreCoreButton.GetComponentInChildren<TMP_Text>().text = $"{selectedCharacter.equipment.armorCore.CurrentLevel}";
        }
        else
        {
            armoreCoreButton.interactable = false;
        }

        if (selectedCharacter.equipment.weapon1 != null)
        {
            weapon1Button.interactable = true;
            weapon1Button.GetComponentInChildren<TMP_Text>().text = $"{selectedCharacter.equipment.weapon1.CurrentLevel}";
        }
        else
        {
            weapon1Button.interactable = false;
        }

        if (selectedCharacter.equipment.weapon2 != null)
        {
            weapon2Button.interactable = true;
            weapon2Button.GetComponentInChildren<TMP_Text>().text = $"{selectedCharacter.equipment.weapon2.CurrentLevel}";
        }
        else
        {
            weapon2Button.interactable = false;
        }
    }

    private void HideCharacterEquipment()
    {
        armoreCoreButton.interactable = true;
        armoreCoreButton.GetComponentInChildren<TMP_Text>().text = $"";
        weapon1Button.interactable = true;
        weapon1Button.GetComponentInChildren<TMP_Text>().text = $"";
        weapon2Button.interactable = true;
        weapon2Button.GetComponentInChildren<TMP_Text>().text = $"";
    }

    public void SelectArmorCore()
    {
        selectedItem = selectedCharacter.equipment.armorCore;
        ShowItemDetails();
    }

    public void SelectWeapon1()
    {
        selectedItem = selectedCharacter.equipment.weapon1;
        ShowItemDetails();
    }

    public void SelectWeapon2()
    {
        selectedItem = selectedCharacter.equipment.weapon2;
        ShowItemDetails();
    }

    public void OnUpgradeSelection(UpgradeCellVisual upgradeCell)
    {
        HideSelectionPanel();
        if (selectedUpgradeCell && selectedUpgradeCell is WeaponUpgradeCellVisual currentSelectedWeaponUpgradeCell)
            currentSelectedWeaponUpgradeCell.Unselect();

        resetUpgradesButton.gameObject.SetActive(false);
        if (upgradeCell is WeaponUpgradeCellVisual weaponUpgradeCell)
        {
            selectedUpgradeCell = weaponUpgradeCell;
            if (weaponUpgradeCell.GetLinkedUpgrade() is WeaponUpgradeSkill weaponUpgradeSkill)
                resetUpgradesButton.gameObject.SetActive(true);
            ShowSelectionPanel();
        }
    }

    private void ShowSelectionPanel()
    {
        ClearPanel(selectionContainer);
        selectionCells.Clear();

        if (selectedUpgradeCell is WeaponUpgradeCellVisual weaponUpgradeCell)
        {
            if (weaponUpgradeCell.GetLinkedUpgrade() is WeaponUpgradeSkill weaponUpgradeSkill && !weaponUpgradeSkill.isFixed)
            {
                ShowSelectionForSkills();
                selectionPanel.SetActive(true);
            }
            else if (weaponUpgradeCell.GetLinkedUpgrade() is WeaponUpgradeRune)
            {
                ShowSelectionForRunes();
                selectionPanel.SetActive(true);
            }
        }
    }

    private void ShowSelectionForSkills()
    {
        if (selectedItem is not Weapon weapon)
            return;

        var availableSkills = weapon.GetAvailableSkills();
        for (int i = 0; i < availableSkills.Count; i++)
        {
            SkillCellVisual newSkillCell = Instantiate(skillCellPrefab, selectionContainer);
            newSkillCell.Setup(availableSkills[i], this);
            selectionCells.Add(newSkillCell);
        }
    }

    private void ShowSelectionForRunes()
    {
        if (selectedItem is not Weapon)
            return;

        Base.Instance.inventory.GetItems(out List<Rune> availableRunes);
        for (int i = 0; i < availableRunes.Count; i++)
        {
            RuneCellVisual newSkillCell = Instantiate(runeCellPrefab, selectionContainer);
            newSkillCell.Setup(availableRunes[i]);
            selectionCells.Add(newSkillCell);
        }
    }

    private void HideSelectionPanel()
    {
        selectionPanel.SetActive(false);
    }

    public void OnSkillSelection(Skill skill)
    {
        if (!selectedUpgradeCell)
            return;

        if (selectedUpgradeCell is WeaponUpgradeCellVisual weaponUpgradeCell)
        {
            if (weaponUpgradeCell.GetLinkedUpgrade() is WeaponUpgradeSkill weaponUpgradeSkill && !weaponUpgradeSkill.isFixed)
                weaponUpgradeSkill.linkedSkill = skill;
        }
        ShowSelectionForSkills();
    }

    public void OnRuneSelection(Rune rune)
    {
        if (!selectedUpgradeCell)
            return;

        if (selectedUpgradeCell is WeaponUpgradeCellVisual weaponUpgradeCell)
        {
            if (weaponUpgradeCell.GetLinkedUpgrade() is WeaponUpgradeRune weaponUpgradeRune)
            {
                if (weaponUpgradeRune.linkedRune != null)
                    Base.Instance.inventory.Add(weaponUpgradeRune.linkedRune);
                Base.Instance.inventory.Remove(rune);
                weaponUpgradeRune.linkedRune = rune;
            }
        }
        ShowSelectionForRunes();
    }

    public void ResetUpgrades()
    {
        if (!selectedUpgradeCell)
            return;

        if (selectedItem is Weapon weapon && selectedUpgradeCell is WeaponUpgradeCellVisual selectedWeaponUpgradeCell)
        {
            if (selectedWeaponUpgradeCell.GetLinkedUpgrade() is WeaponUpgradeSkill weaponUpgradeSkill)
            {
                weapon.ResetUpgrades(weaponUpgradeSkill);
                DisplayWeaponGrid(weapon);
                selectedUpgradeCell = null;
                resetUpgradesButton.gameObject.SetActive(false);
            }
        }
    }

    private void ShowItemDetails()
    {
        HideSelectionPanel();
        resetUpgradesButton.gameObject.SetActive(false);

        upgradeButton.gameObject.SetActive(true);
        upgradeGridPanel.SetActive(false);

        UpdateItemDetails();

        if (selectedItem is ArmorCore armorCore)
        {
            DisplayArmorCoreGrid(armorCore);
        }
        else if (selectedItem is Weapon weapon)
        {
            DisplayWeaponGrid(weapon);
        }

        upgradeGridPanel.SetActive(true);
    }

    private void UpdateItemDetails()
    {
        if (selectedItem is IUpgradable upgradable)
        {
            itemInfo.text = upgradable.GetUpgradeDescription();
        }
    }

    public void TryUpgrade()
    {
        if (selectedItem is IUpgradable upgradable)
        {
            upgradable.Upgrade(out Upgrade upgrade);
            if (upgrade is ArmorCoreUpgrade armorCoreUpgrade)
            {
                AddArmorCoreCell(armorCoreUpgrade);
            }
            if (upgrade is WeaponUpgradeRune weaponUpgrade)
            {
                AddWeaponUpgradeCell(weaponUpgrade, ((Weapon)selectedItem).GetSkillNumberByRuneUpgrade(weaponUpgrade));
            }
            ShowCharacterEquipment();
            UpdateItemDetails();
        }
    }

    private void DisplayLockedCells()
    {
        int xMax, yMax;

        if (selectedItem is Weapon)
        {
            xMax = Weapon.xUpgradeMax;
            yMax = Weapon.yUpgradeMax;
        }
        else if (selectedItem is ArmorCore)
        {
            xMax = ArmorCore.xUpgradeMax;
            yMax = ArmorCore.yUpgradeMax;
        }
        else
        {
            return;
        }

        for (int x = -xMax; x <= xMax; x++)
        {
            for (int y = -yMax; y <= yMax; y++)
            {
                UpgradeCellVisual upgradeCell = Instantiate(lockedUpgradeCellPrefab, upgradeGridContainer);
                Vector2Int position = new(x, y);
                upgradeCell.GetComponent<RectTransform>().anchoredPosition = HexToPixel(position);
                upgradeCells.Add(position, upgradeCell);
            }
        }
    }

    private void DisplayArmorCoreGrid(ArmorCore armorCore)
    {
        ClearHexGrid();
        DisplayLockedCells();

        var upgrades = armorCore.GetUpgrades();
        foreach (var upgrade in upgrades)
        {
            AddArmorCoreCell(upgrade);
        }
    }

    private void AddArmorCoreCell(Upgrade armorCoreUpgrade)
    {
        ArmorCoreUpgradeCellVisual upgradeCell = Instantiate(armorCoreUpgradeCellPrefab, upgradeGridContainer);
        upgradeCell.GetComponent<RectTransform>().anchoredPosition = HexToPixel(armorCoreUpgrade.gridPosition);
        upgradeCell.Setup(armorCoreUpgrade);

        RemoveLockedCell(armorCoreUpgrade.gridPosition);
        upgradeCells.Add(armorCoreUpgrade.gridPosition, upgradeCell);
    }

    private void DisplayWeaponGrid(Weapon weapon)
    {
        ClearHexGrid();
        DisplayLockedCells();

        var upgradesDictionary = weapon.GetUprades();
        var upgradeSkills = upgradesDictionary.Keys.ToList();
        var upgradeRunes = upgradesDictionary.Values.ToList();
        for (int i = 0; i < upgradeSkills.Count; i++)
        {
            AddWeaponUpgradeCell(upgradeSkills[i], i);
            foreach (var upgrade in upgradeRunes[i])
            {
                AddWeaponUpgradeCell(upgrade, i);
            }
        }
    }

    private void AddWeaponUpgradeCell(WeaponUpgrade weaponUpgrade, int colorNumber)
    {
        WeaponUpgradeCellVisual upgradeCell = Instantiate(weaponUpgradeCellPrefab, upgradeGridContainer);
        upgradeCell.GetComponent<RectTransform>().anchoredPosition = HexToPixel(weaponUpgrade.gridPosition);

        upgradeCell.Setup(weaponUpgrade, colorNumber);

        RemoveLockedCell(weaponUpgrade.gridPosition);

        upgradeCells.Add(weaponUpgrade.gridPosition, upgradeCell);
    }

    private void RemoveLockedCell(Vector2Int position)
    {
        upgradeCells.TryGetValue(position, out UpgradeCellVisual lockedCell);
        Destroy(lockedCell);
        upgradeCells.Remove(position);
    }

    private Vector2 HexToPixel(Vector2Int hexPosition)
    {
        float x = (hexPosition.x + Math.Abs(hexPosition.y) * 0.5f - Math.Abs(hexPosition.y) / 2) * (cellInnerRadius * 2f);
        float y = hexPosition.y * (cellOuterRadius * 1.5f);
        return new Vector2(x, y) + upgradeGridContainerCenter;
    }

    private void ClearHexGrid()
    {
        ClearPanel(upgradeGridContainer);
        upgradeCells.Clear();
    }
}