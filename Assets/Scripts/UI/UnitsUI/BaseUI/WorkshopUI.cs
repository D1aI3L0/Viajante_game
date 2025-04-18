using System.Collections.Generic;
using UnityEngine;

public class WorkshopUI : MonoBehaviour
{
    public static WorkshopUI Instance { get; private set; }
    
    [Header("UI References")]
    public EquipmentUpgradeUI upgradeUI;
    
    [Header("Materials")]
    public int upgradeMaterialAmount = 100;
    
    private PlayerCharacter selectedCharacter;
    private Item selectedItem;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        
        Instance = this;
    }
    
    public void OpenWorkshop(PlayerCharacter character)
    {
        selectedCharacter = character;
        upgradeUI.Show(character);
    }
    
    public void SelectItem(Item item)
    {
        selectedItem = item;
        upgradeUI.ShowItemDetails(item);
    }
    
    public void TryUpgradeItem()
    {
        if (selectedItem == null) return;
        
        if (selectedItem is IUpgradable upgradableItem)
        {
            if (upgradeMaterialAmount >= upgradableItem.GetUpgradeCost())
            {
                upgradeMaterialAmount -= upgradableItem.GetUpgradeCost();
                upgradableItem.Upgrade();
                upgradeUI.ShowItemDetails(selectedItem);
                upgradeUI.UpdateMaterialsDisplay(upgradeMaterialAmount);
            }
            else
            {
                Debug.Log("Not enough materials!");
            }
        }
    }
    
    public void AddMaterials(int amount)
    {
        upgradeMaterialAmount += amount;
        upgradeUI.UpdateMaterialsDisplay(upgradeMaterialAmount);
    }
}