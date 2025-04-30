using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Unity.VisualScripting;

public class SquadCreationUI : MonoBehaviour
{
    [Header("Основные элементы")]
    public GameObject squadCreationPanel;
    public TMP_Text squadSizeText;

    [Header("Панели персонажей")]
    public Transform availableCharactersPanel;
    public Transform squadMembersPanel;

    [Header("Префабы")]
    public CharacterSlotBase characterSlotPrefab;

    [Header("Кнопки")]
    public Button createSquadButton;
    public Button cancelButton;

    private List<PlayerCharacter> selectedCharacters = new List<PlayerCharacter>();

    public void Show()
    {
        enabled = true;
        squadCreationPanel.SetActive(true);

        selectedCharacters.Clear();
        RefreshAvailableCharacters();
        RefreshSquadMembers();
        UpdateUI();
    }

    public void Hide()
    {
        UIReferences.gameUI.enabled = true;
        enabled = false;
        squadCreationPanel.SetActive(false);
    }

    void RefreshAvailableCharacters()
    {
        ClearPanel(availableCharactersPanel);

        foreach (PlayerCharacter character in Base.Instance.availableCharacters)
        {
            if (!selectedCharacters.Contains(character))
            {
                CharacterSlotBase slot = Instantiate(characterSlotPrefab, availableCharactersPanel);
                slot.Initialize(character, this, false);
            }
        }
    }

    void RefreshSquadMembers()
    {
        ClearPanel(squadMembersPanel);

        foreach (PlayerCharacter character in selectedCharacters)
        {
            CharacterSlotBase slot = Instantiate(characterSlotPrefab, squadMembersPanel);
            slot.Initialize(character, this, true);
        }

        UpdateUI();
    }

    void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }

    public void ToggleCharacterSelection(PlayerCharacter character, bool isSelected)
    {
        if (isSelected && selectedCharacters.Count < 3)
        {
            if (!selectedCharacters.Contains(character))
                selectedCharacters.Add(character);
        }
        else
        {
            selectedCharacters.Remove(character);
        }

        RefreshAvailableCharacters();
        RefreshSquadMembers();
    }

    void UpdateUI()
    {
        squadSizeText.text = $"{selectedCharacters.Count}/3";
        createSquadButton.interactable = selectedCharacters.Count > 0;
    }

    public void CreateSquad()
    {
        HexCell cell = null;
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            HexCell neighbor = Base.Instance.Location.GetNeighbor(d);
            if (neighbor && Squad.SquadValidDestionation(neighbor) && !neighbor.HasUnit)
            {
                cell = neighbor;
                break;
            }
        }
        if (cell)
            Base.Instance.CreateSquad(cell, selectedCharacters);
        else
            Debug.LogWarning("No available cells to create squad");
        Hide();
    }
}