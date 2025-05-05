using UnityEngine;

[CreateAssetMenu(fileName = "NewTrait", menuName = "Traits/Trait")]
public class TraitData : ScriptableObject
{
    public string traitName;
    public Sprite icon;
    [TextArea] 
    public string description;
    public TraitType traitType;
    public TraitEffect[] effects;
    public bool isPermanent;
    public int durationTurns; // 0 = permanent
}

public enum TraitType
{
    Positive,
    Negatine
}