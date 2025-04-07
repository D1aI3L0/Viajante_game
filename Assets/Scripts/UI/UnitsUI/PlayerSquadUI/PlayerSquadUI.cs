using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlayerSquadUI : MonoBehaviour
{
    [Header("Основные элементы")]
    public GameObject playerSuqadPanel;
    public TMP_Text squadNameText;
    public Transform charactersContainer;
    public CharacterSlotSquad characterSlotPrefab;

    [Header("Кнопки")]
    public Button returnToBaseButton;
    public Image teleportScrollIcon;

    private Squad currentSquad;
    private List<CharacterSlotSquad> characterSlots = new List<CharacterSlotSquad>();

    [Header("Главное UI")]
    public HexUI hexUI;

    void Start()
    {
        playerSuqadPanel.SetActive(false);
        enabled = false;
    }

    public void ShowForSquad(Squad squad)
    {
        playerSuqadPanel.SetActive(true);
        currentSquad = squad;
        UpdateUI();
        enabled = true;
    }

    public void Hide()
    {
        playerSuqadPanel.SetActive(false);
        enabled = false;
    }

    void UpdateUI()
    {
        squadNameText.text = currentSquad.name;

        foreach (Transform child in charactersContainer)
            Destroy(child.gameObject);

        characterSlots.Clear();

        foreach (Character character in currentSquad.characters)
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
            hexUI.gameUI.selectedUnit = null;
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