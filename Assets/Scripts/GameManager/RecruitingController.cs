using System.Collections.Generic;
using System.IO;
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

    public static BasicCharacterTemplates GetCharacterTemplate(CharacterClass characterClass)
    {
        foreach(BasicCharacterTemplates characterTemplate in Instance.characterTemplates)
        {
            if(characterTemplate.characterClass == characterClass)
            {
                return characterTemplate;
            }
        }
        return null;
    }

    public int GetTraitID(TraitData traitData)
    {
        if(traitData.traitType == TraitType.Positive)
        {
            for(int i = 0; i < availablePositiveTraits.Count; i++)
            {
                if(availablePositiveTraits[i] == traitData)
                    return i;
            }
        }
        else
        {
            for(int i = 0; i < availableNegativeTraits.Count; i++)
            {
                if(availableNegativeTraits[i] == traitData)
                    return i;
            }
        }
        return -1;
    }

    public TraitData GetPositiveTraitByID(int id)
    {
        if(id < 0 || id > availablePositiveTraits.Count)
            return null;

        return availablePositiveTraits[id];
    }

    public TraitData GetNegativeTraitByID(int id)
    {
        if(id < 0 || id > availableNegativeTraits.Count)
            return null;

        return availableNegativeTraits[id];
    }


    public void Save(BinaryWriter writer)
    {
        writer.Write(recruitingCharacters.Count);
        foreach(PlayerCharacter character in recruitingCharacters)
        {
            character.Save(writer);
        }
    }

    public void Load(BinaryReader reader)
    {
        int charactersCount = reader.ReadInt32();
        for(int i = 0; i < charactersCount; i++)
        {
            PlayerCharacter newCharacter = new();
            newCharacter.Load(reader);
            recruitingCharacters.Add(newCharacter);
        }
    }
}
