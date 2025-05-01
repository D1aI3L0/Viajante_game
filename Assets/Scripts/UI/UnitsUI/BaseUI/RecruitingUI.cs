using System.Collections.Generic;
using UnityEngine;

public class RecruitingUI : MonoBehaviour
{
    public static RecruitingUI Instance;
    public GameObject recrutingPanel;

    public RectTransform recruitingCharactersContainer;

    private List<PlayerCharacter> recruitingCharacters = new();

    public List<BasicCharacterTemplates> characterTemplates;

    public void Awake()
    {
        Instance = this;
        Hide();
    }

    public void Hide()
    {
        recrutingPanel.SetActive(false);
    }

    public void Show()
    {
        recrutingPanel.SetActive(false);
    }

    public void UpdateRecruitingCharacters()
    {
        recruitingCharacters.Clear();
        ClearPanel(recruitingCharactersContainer);
    }

    void ClearPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Destroy(child.gameObject);
        }
    }
}