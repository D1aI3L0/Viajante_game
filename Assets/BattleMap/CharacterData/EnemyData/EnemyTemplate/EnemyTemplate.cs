using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTemplate", menuName = "Enemy/EnemyTemplate")]
public class EnemyTemplate : ScriptableObject
{
    public EnemyStats enemyStats = new();
}
