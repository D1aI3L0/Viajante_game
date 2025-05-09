using System.Collections.Generic;
using UnityEngine;

public class RecruitingController : MonoBehaviour
{
    public static RecruitingController Instance;

    private List<PlayerCharacter> recruitingCharacters = new();
    public List<BasicCharacterTemplates> characterTemplates;
    public List<TraitData> availablePositiveTraits, availableNegativeTraits;

    private const int recruitmentCharactersPerCycleCount = 4;
    private const int maxPositiveTraitsCount = 2;
    private const int maxNegativeTraitsCount = 2;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RecruitingUI.Instance.recruitingController = this;
    }

    public void UpdateRecruitingCharacters()
    {
        recruitingCharacters.Clear();
        RecruitingUI.Instance.UpdateCharactersPanel();

        if (characterTemplates.Count > 0)
        {
            for (int i = 0; i < recruitmentCharactersPerCycleCount; i++)
            {
                PlayerCharacter newCharacter = new();
                newCharacter.Initialize(characterTemplates[UnityEngine.Random.Range(0, characterTemplates.Count)]);
                recruitingCharacters.Add(newCharacter);

                if (availablePositiveTraits != null && availablePositiveTraits.Count > 0)
                {
                    int positiveTraitsCount = UnityEngine.Random.Range(0, maxPositiveTraitsCount + 1);
                    for (int j = 0; j < positiveTraitsCount; j++)
                    {
                        TraitData randomTraitData = availablePositiveTraits[UnityEngine.Random.Range(0, availablePositiveTraits.Count)];
                        if(!newCharacter.HasTrait(randomTraitData))
                            newCharacter.AddTrait(randomTraitData);
                    }
                }

                if (availableNegativeTraits != null && availableNegativeTraits.Count > 0)
                {
                    int negativeTraitsCount = UnityEngine.Random.Range(0, maxNegativeTraitsCount + 1);
                    for (int j = 0; j < negativeTraitsCount; j++)
                    {
                        TraitData randomTraitData = availableNegativeTraits[UnityEngine.Random.Range(0, availableNegativeTraits.Count)];
                        if(!newCharacter.HasTrait(randomTraitData))
                            newCharacter.AddTrait(randomTraitData);
                    }
                }
            }
        }
    }

    public List<PlayerCharacter> GetCharacters()
    {
        return recruitingCharacters;
    }

    public void Remove(PlayerCharacter playerCharacter)
    {
        recruitingCharacters.Remove(playerCharacter);
    }
}
