using UnityEngine;

[CreateAssetMenu(fileName = "BattleConfig", menuName = "Battle/Battle Config", order = 1)]
public class BattleConfig : ScriptableObject
{
    [Header("Параметры карты")]
    [Tooltip("Размер карты, где X - количество ячеек по горизонтали, а Y - количество ячеек по вертикали.")]
    public Vector2Int battleMapSize = new Vector2Int(20, 20);

    [Header("Параметры противников")]
    [Tooltip("Количество врагов, которые будут создаваться на боевой карте.")]
    public int enemyCount = 5;

    [Header("Параметры препятствий")]
    [Tooltip("Процент заполненности карты препятствиями (значение от 0 до 1, например, 0.05 означает 5% ячеек).")]
    public float obstaclePercent = 0.05f;

    [Header("Дополнительные параметры")]
    [Tooltip("Уровень сложности боя (например, от 1 до 10), который может влиять на поведение и характеристики противников.")]
    public int battleDifficulty = 1;
}
