using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerSquadUI : MonoBehaviour
{
    [Header("Основные элементы")]
    public GameObject UIContainer;
    public Transform charactersContainer;
    public CharacterSlotSquad characterSlotPrefab;

    [Header("Кнопки")]
    public Button returnToBaseButton;

    private Squad currentSquad;
    private List<CharacterSlotSquad> characterSlots = new List<CharacterSlotSquad>();

    void Awake()
    {
        UIReferences.playerSquadUI = this;
        UIContainer.SetActive(false);
        enabled = false;
    }

    public void ShowForSquad(Squad squad)
    {
        currentSquad = squad;
        UpdateUI();
        enabled = true;
        UIContainer.SetActive(true);
    }

    public void Hide()
    {
        UIContainer.SetActive(false);
        enabled = false;
    }

    void UpdateUI()
    {
        foreach (Transform child in charactersContainer)
            Destroy(child.gameObject);

        characterSlots.Clear();

        foreach (PlayerCharacter character in currentSquad.characters)
        {
            CharacterSlotSquad slot = Instantiate(characterSlotPrefab, charactersContainer);
            slot.Initialize(character);
            characterSlots.Add(slot);
        }
    }

    public void ReturnToBase()
    {
        if (currentSquad != null)
        {
            currentSquad.ReturnToBase();
            UIReferences.gameUI.selectedUnit = null;
            Hide();
        }
    }

    void Update()
    {
        foreach (var slot in characterSlots)
        {
            slot.UpdateStats();
        }
    }
}