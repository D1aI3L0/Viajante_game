using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerCharacterUI : MonoBehaviour
{
    public GameObject UIContainer;
    public PerkSlot perkSlotPrefab;
    public SkillSlot skillSlotPrefab;

    public GameObject armorCoreSlot, artifactSlot, weapon1Slot, weapon2Slot;
    public Transform skills1Container, skills2Container, positivePerksContainer, negativePerksContainer;

    private List<SkillSlot> skill1 = new(), skill2 = new();
    private List<PerkSlot> positivePerks = new(), negativePerks = new();

    public TMP_Text healthLabel, defenceLabel, evasionLabel, attackLabel, accurancyLabel, critLabel, enduranceAmountLabel, enduranceRegenLabel, enduranceMoveCostLabel, initiativeLabel, agroLabel;

    private PlayerCharacter playerCharacter;


    void Awake()
    {
        UIReferences.playerCharacterUI = this;
        enabled = false;
        UIContainer.SetActive(false);
        playerCharacter = null;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
            Hide();
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
        enabled = false;
        UIContainer.SetActive(false);
    }

    public void UpdateUI()
    {
        foreach (Transform child in skills1Container)
            Destroy(child.gameObject);
        foreach (Transform child in skills2Container)
            Destroy(child.gameObject);
        foreach (Transform child in positivePerksContainer)
            Destroy(child.gameObject);
        foreach (Transform child in negativePerksContainer)
            Destroy(child.gameObject);

        skill1.Clear();
        skill2.Clear();
        positivePerks.Clear();
        negativePerks.Clear();



        UpdateInfo();
    }

    private void UpdateInfo()
    {
        UpdateSurvivalInfo();
        UpdateAttackInfo();
        UpdateOtherInfo();
    }

    private void UpdateSurvivalInfo()
    {
        healthLabel.text = $"{playerCharacter.currentSurvivalStats.health}/{playerCharacter.maxSurvivalStats.health}";
        defenceLabel.text = $"{playerCharacter.currentSurvivalStats.defence}";
        evasionLabel.text = $"{playerCharacter.maxSurvivalStats.evasion}";
    }

    private void UpdateAttackInfo()
    {
        attackLabel.text = $"{playerCharacter.currentAttack1Stats.attack}/{playerCharacter.currentAttack2Stats.attack}";
        accurancyLabel.text = $"{playerCharacter.currentAttack1Stats.accuracy}/{playerCharacter.currentAttack2Stats.accuracy}";
        critLabel.text = $"{playerCharacter.currentAttack1Stats.critRate}/{playerCharacter.currentAttack2Stats.critRate}";
    }

    private void UpdateOtherInfo()
    {
        enduranceAmountLabel.text = $"{playerCharacter.currentOtherStats.endurance.amount}";
        enduranceRegenLabel.text = $"{playerCharacter.currentOtherStats.endurance.regen}";
        enduranceMoveCostLabel.text = $"{playerCharacter.currentOtherStats.endurance.moveCost}";
        initiativeLabel.text = $"{playerCharacter.currentOtherStats.initiative}";
        agroLabel.text = $"{playerCharacter.currentOtherStats.agro}";
    }
}
