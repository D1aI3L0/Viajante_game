using UnityEngine;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "Battle/Battle Config", order = 1)]
public class BattleConfig : ScriptableObject
{
    [Header("Параметры карты X-ширина Y-высота(Z)")]
    public Vector2Int battleMapSize = new Vector2Int(20, 20);
    
    [Header("Параметры противников и препятствий")]
    public int enemyCount;
    public float obstaclePercent;
}
