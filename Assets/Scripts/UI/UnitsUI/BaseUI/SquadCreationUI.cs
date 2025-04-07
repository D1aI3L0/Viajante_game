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
    public CharacterSlot characterSlotPrefab;

    [Header("Кнопки")]
    public Button createSquadButton;
    public Button cancelButton;

    Base currentBase;
    List<Character> selectedCharacters = new List<Character>();

    [Header("Главное UI")]
    public HexUI hexUI;

    public void Initialize(Base playerBase)
    {
        currentBase = playerBase;
        selectedCharacters.Clear();
        RefreshAvailableCharacters();
        RefreshSquadMembers();
        UpdateUI();
    }

    void RefreshAvailableCharacters()
    {
        ClearPanel(availableCharactersPanel);

        foreach (Character character in currentBase.availableCharacters)
        {
            if (!selectedCharacters.Contains(character))
            {
                CharacterSlot slot = Instantiate(characterSlotPrefab, availableCharactersPanel);
                slot.Initialize(character, this, false);
            }
        }
    }

    void RefreshSquadMembers()
    {
        ClearPanel(squadMembersPanel);

        foreach (Character character in selectedCharacters)
        {
            CharacterSlot slot = Instantiate(characterSlotPrefab, squadMembersPanel);
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

    public void ToggleCharacterSelection(Character character, bool isSelected)
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
            HexCell neighbor = currentBase.Location.GetNeighbor(d);
            if (neighbor && Squad.SquadValidDestionation(neighbor) && !neighbor.HasUnit)
            {
                cell = neighbor;
                break;
            }
        }
        if (cell)
            currentBase.CreateSquad(cell, selectedCharacters);
        else
            Debug.LogWarning("No available cells to create squad");
        ClosePanel();
    }

    public void ClosePanel()
    {
        hexUI.gameUI.enabled = true;
        this.GameObject().SetActive(false);
    }
}