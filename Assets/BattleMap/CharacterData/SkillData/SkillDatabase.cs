using UnityEngine;

public class SkillDatabase : MonoBehaviour
{
    public static SkillDatabase Instance;

    [SerializeField] private SkillAsset[] allSkills;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public SkillAsset GetSkillById(int skillId)
    {
        foreach (SkillAsset skill in allSkills)
        {
            if (skill.GetInstanceID() == skillId)
            {
                return skill;
            }
        }
        return null;
    }
}