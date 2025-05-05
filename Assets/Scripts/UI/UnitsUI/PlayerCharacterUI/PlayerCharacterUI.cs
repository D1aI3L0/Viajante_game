using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCharacterUI : MonoBehaviour
{
    public GameObject UIContainer;

    public GameObject armorCoreSlot, artifactSlot;
    public WeaponCellVisual weapon1Cell, weapon2Cell;

    public TraitCellVisual traitCellPrefab;
    public SkillCellVisual skillCellPrefab;
    public Transform skills1Container, skills2Container, positiveTraitsContainer, negativeTraitsContainer;

    private List<SkillCellVisual> skills1 = new(), skills2 = new();
    private List<TraitCellVisual> positiveTraits = new(), negativeTraits = new();

    public TMP_Text healthLabel, defenceLabel, evasionLabel, attackLabel, accurancyLabel, critLabel, SPAmountLabel, SPRegenLabel, SPMoveCostLabel, speedLabel, tountLabel;

    public Image specialEnergy1, specialEnergy2, specialEnergyCombined;
    public TMP_Text specialEnergy1Amount, specialEnergy2Amount, specialEnergyCombinedAmount;

    private PlayerCharacter playerCharacter; 

    void Awake()
    {
        UIReferences.playerCharacterUI = this;
        enabled = false;
        UIContainer.SetActive(false);
        playerCharacter = null;
    }

    public void ShowForCharacter(PlayerCharacter character)
    {
        playerCharacter = character;
        UpdateUI();
        enabled = true;
        UIContainer.SetActive(true);
    }

    public void Hide()
    {
        HideSpecialEnergies();
        enabled = false;
        UIContainer.SetActive(false);
    }

    private void HideSpecialEnergies()
    {
        specialEnergy1.gameObject.SetActive(false);
        specialEnergy2.gameObject.SetActive(false);
        specialEnergyCombined.gameObject.SetActive(false);
    }

    public void UpdateUI()
    {
        ClearPanel(skills1Container);
        ClearPanel(skills2Container);
        ClearPanel(positiveTraitsContainer);
        ClearPanel(negativeTraitsContainer);

        HideSpecialEnergies();

        skills1.Clear();
        skills2.Clear();
        positiveTraits.Clear();
        negativeTraits.Clear();

        UpdateInfo();
    }

    void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }

    private void UpdateInfo()
    {
        weapon1Cell.Setup(playerCharacter.equipment.weapon1);
        weapon2Cell.Setup(playerCharacter.equipment.weapon2);
        UpdateSurvivalInfo();
        UpdateAttackInfo();
        UpdateOtherInfo();
        UpdateSkillsInfo();
        UpdateTraitsInfo();
    }

    private void UpdateSurvivalInfo()
    {
        healthLabel.text = $"{playerCharacter.currentCharacterStats.maxHealth}/{playerCharacter.currentCharacterStats.maxHealth}";
        defenceLabel.text = $"{playerCharacter.currentCharacterStats.defence}";
        evasionLabel.text = $"{playerCharacter.currentCharacterStats.evasion}";
    }

    private void UpdateAttackInfo()
    {
        attackLabel.text = $"{playerCharacter.currentAttack1Stats.attack}/{playerCharacter.currentAttack2Stats.attack}";
        accurancyLabel.text = $"{playerCharacter.currentAttack1Stats.accuracy}/{playerCharacter.currentAttack2Stats.accuracy}";
        critLabel.text = $"{playerCharacter.currentAttack1Stats.critRate}/{playerCharacter.currentAttack2Stats.critRate}";
    }

    private void UpdateOtherInfo()
    {
        SPAmountLabel.text = $"{playerCharacter.baseCharacterStats.SPamount}";
        SPRegenLabel.text = $"{playerCharacter.baseCharacterStats.SPregen}";
        SPMoveCostLabel.text = $"{playerCharacter.baseCharacterStats.SPmoveCost}";
        speedLabel.text = $"{playerCharacter.baseCharacterStats.speed}";
        tountLabel.text = $"{playerCharacter.baseCharacterStats.tount}";
    }

    private void UpdateSkillsInfo()
    {
        ClearPanel(skills1Container);

        for (int i = 0; i < playerCharacter.equipment.weapon1.skills.Count; i++)
        {
            SkillCellVisual skillCell = Instantiate(skillCellPrefab, skills1Container);
            skillCell.Setup(playerCharacter.equipment.weapon1.skills[i]);
            skills1.Add(skillCell);
        }
        specialEnergy1.gameObject.SetActive(true);
        specialEnergy1.color = SpecialEnergy.SpecialEnergyColors[0];
        specialEnergy1Amount.text = $"{playerCharacter.equipment.weapon1.specialEnergy.amount}";

        ClearPanel(skills2Container);

        for (int i = 0; i < playerCharacter.equipment.weapon2.skills.Count; i++)
        {
            SkillCellVisual skillCell = Instantiate(skillCellPrefab, skills2Container);
            skillCell.Setup(playerCharacter.equipment.weapon2.skills[i]);
            skills2.Add(skillCell);
        }

        specialEnergy2.gameObject.SetActive(true);
        specialEnergy2.color = SpecialEnergy.SpecialEnergyColors[0];
        specialEnergy2Amount.text = $"{playerCharacter.equipment.weapon2.specialEnergy.amount}";
    }

    private void UpdateTraitsInfo()
    {

    }
}
