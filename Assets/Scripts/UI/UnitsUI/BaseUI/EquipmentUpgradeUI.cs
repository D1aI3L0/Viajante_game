using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EquipmentUpgradeUI : MonoBehaviour
{
    [Header("Основные элементы")]
    public GameObject panel;
    public Button closeButton;
    
    [Header("Выбор персонажа")]
    public TMP_Dropdown characterDropdown;
    public Image characterPortrait;
    public TMP_Text characterStats;
    
    [Header("Выбор предмета")]
    public Transform itemsContainer;
    public GameObject itemButtonPrefab;
    
    [Header("Детали предмета")]
    public GameObject itemDetailsPanel;
    public Image itemIcon;
    public TMP_Text itemName;
    public TMP_Text itemDescription;
    public TMP_Text itemLevel;
    public TMP_Text upgradeCost;
    public Button upgradeButton;
    
    [Header("Материалы")]
    public TMP_Text materialsText;
    
    private List<PlayerCharacter> availableCharacters = new List<PlayerCharacter>();
    private PlayerCharacter selectedCharacter;
    private Item selectedItem;

    [Header("Сетка прокачки брони")]
    public GameObject hexGridPanel;
    public RectTransform hexGridContainer;
    public GameObject hexCellPrefab;
    public float hexSize = 50f;
    public float hexWidth = 86.6f;
    public float hexHeight = 100f;
    
    private Dictionary<Vector2Int, GameObject> hexCells = new Dictionary<Vector2Int, GameObject>();
    
    private void Awake()
    {
        closeButton.onClick.AddListener(Hide);
        upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }
    
    public void Show(PlayerCharacter character = null)
    {
        panel.SetActive(true);
        
        if (character != null)
        {
            selectedCharacter = character;
            ShowCharacterEquipment(character);
        }
        else
        {
            PopulateCharacterDropdown();
        }
        
        UpdateMaterialsDisplay(WorkshopUI.Instance.upgradeMaterialAmount);
    }
    
    public void Hide()
    {
        panel.SetActive(false);
        itemDetailsPanel.SetActive(false);
    }
    
    private void PopulateCharacterDropdown()
    {
        characterDropdown.ClearOptions();
        availableCharacters.Clear();
        
        var baseCharacters = Base.Instance?.availableCharacters;
        if (baseCharacters == null || baseCharacters.Count == 0) return;
        
        availableCharacters.AddRange(baseCharacters);
        
        List<string> options = new List<string>();
        foreach (var character in availableCharacters)
        {
            options.Add(character.characterName);
        }
        
        characterDropdown.AddOptions(options);
        characterDropdown.onValueChanged.AddListener(OnCharacterSelected);
        
        if (availableCharacters.Count > 0)
        {
            OnCharacterSelected(0);
        }
    }
    
    private void OnCharacterSelected(int index)
    {
        if (index < 0 || index >= availableCharacters.Count) return;
        
        selectedCharacter = availableCharacters[index];
        ShowCharacterEquipment(selectedCharacter);
    }
    
    private void ShowCharacterEquipment(PlayerCharacter character)
    {
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }
        
        characterStats.text = $"Уровень: {character.level}\n" +
                            $"Здоровье: {character.currentSurvivalStats.health}\n" +
                            $"Атака: {character.currentAttack1Stats.attack}";
        
        CreateEquipmentButton("Броня", character.equipment.armorCore);
        CreateEquipmentButton("Оружие 1", character.equipment.weapon1);
        CreateEquipmentButton("Оружие 2", character.equipment.weapon2);
        CreateEquipmentButton("Артефакт", character.equipment.artifact);
    }
    
    private void CreateEquipmentButton(string slotName, Item item)
    {
        var buttonObj = Instantiate(itemButtonPrefab, itemsContainer);
        var button = buttonObj.GetComponent<Button>();
        var buttonText = buttonObj.GetComponentInChildren<TMP_Text>();
        
        buttonText.text = slotName;
        
        if (item == null)
        {
            button.interactable = false;
            buttonText.text += " (not equiped)";
            return;
        }
        
        button.onClick.AddListener(() => ShowItemDetails(item));
    }
    
    public void ShowItemDetails(Item item)
    {
        selectedItem = item;
        itemDetailsPanel.SetActive(true);
        
        itemIcon = item.icon;
        itemName.text = item.name;
        
        hexGridPanel.SetActive(false);
        
        if (item is ArmorCore armorCore)
        {
            hexGridPanel.SetActive(true);
            DisplayArmorCoreGrid(armorCore);
            
            itemLevel.text = $"Уровень: {armorCore.CurrentLevel}/{armorCore.MaxLevel}";
            upgradeCost.text = $"Стоимость: {armorCore.GetUpgradeCost()} материалов";
            upgradeButton.interactable = armorCore.CurrentLevel < armorCore.MaxLevel;
            
            itemDescription.text = armorCore.GetUpgradeDescription();
        }
        else if (item is IUpgradable upgradable)
        {
            // Для других улучшаемых предметов стандартный UI
            itemLevel.text = $"Уровень: {upgradable.CurrentLevel}/{upgradable.MaxLevel}";
            upgradeCost.text = $"Стоимость: {upgradable.GetUpgradeCost()} материалов";
            upgradeButton.interactable = upgradable.CurrentLevel < upgradable.MaxLevel;
            
            itemDescription.text = upgradable.GetUpgradeDescription();
        }
        else
        {
            itemLevel.text = "Не улучшаемый";
            upgradeCost.text = "";
            upgradeButton.interactable = false;
            itemDescription.text = "Этот предмет нельзя улучшить";
        }
    }

    private void DisplayArmorCoreGrid(ArmorCore armorCore)
    {
        ClearHexGrid();
        
        var upgrades = armorCore.GetUpgrades();
        foreach (var upgrade in upgrades)
        {
            var hexCell = Instantiate(hexCellPrefab, hexGridContainer);
            hexCell.GetComponent<RectTransform>().anchoredPosition = HexToPixel(upgrade.gridPosition);
            
            var visual = hexCell.GetComponent<ArmorCoreHexCellVisual>();
            if (visual != null)
            {
                visual.Setup(upgrade);
            }
            
            hexCells.Add(upgrade.gridPosition, hexCell);
        }
    }

    private Vector2 HexToPixel(Vector2Int hexPosition)
    {
        float x = hexPosition.x * hexWidth;
        float y = hexPosition.y * hexHeight + (hexPosition.x % 2) * hexHeight / 2;
        return new Vector2(x, y);
    }
    
    private void ClearHexGrid()
    {
        foreach (var cell in hexCells.Values)
        {
            Destroy(cell);
        }
        hexCells.Clear();
    }
    
    private void OnUpgradeClicked()
    {
        if (selectedItem is IUpgradable)
        {
            WorkshopUI.Instance.TryUpgradeItem();
        }
    }
    
    public void UpdateMaterialsDisplay(int amount)
    {
        materialsText.text = $"Материалы: {amount}";
    }
}