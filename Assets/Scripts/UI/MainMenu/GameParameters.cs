using UnityEngine;

[CreateAssetMenu(fileName = "GameParameters", menuName = "GameParameters")]
public class GameParameters : ScriptableObject
{
    public string gameName;

    public bool isNewGame;
}
