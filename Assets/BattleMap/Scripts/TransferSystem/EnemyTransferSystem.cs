using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTransferSystem", menuName = "Enemy/Enemy Transfer System")]
public class EnemyTransferSystem : ScriptableObject
{
    [Range(1, 6)]
    public int enemiesCount = 3;

    public EnemyStats[] enemyStats = new EnemyStats[0];
    public EnemyCharacter[] enemyCharacters = new EnemyCharacter[0];

#if UNITY_EDITOR
    private void OnValidate()
    {
        enemyStats = new EnemyStats[enemiesCount];

        if (enemyCharacters.Length > 0)
        {
            // Проходим по количеству персонажей, заданному в numberOfCharacters.
            for (int i = 0; i < enemiesCount; i++)
            {
                if (enemyCharacters[i] != null)
                {
                    if (enemyStats[i] == null)
                        enemyStats[i] = new EnemyStats();

                    // Копирование общих данных из базового ассета.
                    enemyStats[i].characterParameters.maxHP = enemyStats[i].characterParameters.currentHP = enemyCharacters[i].currentCharacterStats.maxHealth;
                    enemyStats[i].characterParameters.DEF = enemyCharacters[i].currentCharacterStats.defence;
                    enemyStats[i].characterParameters.EVA = enemyCharacters[i].currentCharacterStats.evasion;
                    enemyStats[i].characterParameters.PROV = enemyCharacters[i].currentCharacterStats.tount;
                    enemyStats[i].characterParameters.SPD = enemyCharacters[i].currentCharacterStats.speed;
                    enemyStats[i].characterParameters.SP = enemyCharacters[i].currentCharacterStats.SPamount;
                    enemyStats[i].characterParameters.SPreg = enemyCharacters[i].currentCharacterStats.SPregen;
                    enemyStats[i].characterParameters.SPmovecost = enemyCharacters[i].currentCharacterStats.SPmoveCost;

                    enemyStats[i].weaponParameters.ATK = enemyCharacters[i].weaponParameters.ATK;
                    enemyStats[i].weaponParameters.ACC = enemyCharacters[i].weaponParameters.ACC;
                    enemyStats[i].weaponParameters.CRIT = enemyCharacters[i].weaponParameters.CRIT;
                    enemyStats[i].weaponParameters.SE = enemyCharacters[i].weaponParameters.SE;
                    enemyStats[i].weaponParameters.SEreg = enemyCharacters[i].weaponParameters.SEreg;
                    enemyStats[i].weaponParameters.SEdec = enemyCharacters[i].weaponParameters.SEdec;

                    enemyStats[i].skillSet = new(enemyCharacters[i].skillSet);
                }
            }
        }
    }
#endif

    public void Transfer()
    { 
        enemyStats = new EnemyStats[enemiesCount];

        if (enemyCharacters.Length > 0)
        {
            // Проходим по количеству персонажей, заданному в numberOfCharacters.
            for (int i = 0; i < enemiesCount; i++)
            {
                if (enemyCharacters[i] != null)
                {
                    if (enemyStats[i] == null)
                        enemyStats[i] = new EnemyStats();

                    // Копирование общих данных из базового ассета.
                    enemyStats[i].characterParameters.maxHP = enemyStats[i].characterParameters.currentHP = enemyCharacters[i].currentCharacterStats.maxHealth;
                    enemyStats[i].characterParameters.DEF = enemyCharacters[i].currentCharacterStats.defence;
                    enemyStats[i].characterParameters.EVA = enemyCharacters[i].currentCharacterStats.evasion;
                    enemyStats[i].characterParameters.PROV = enemyCharacters[i].currentCharacterStats.tount;
                    enemyStats[i].characterParameters.SPD = enemyCharacters[i].currentCharacterStats.speed;
                    enemyStats[i].characterParameters.SP = enemyCharacters[i].currentCharacterStats.SPamount;
                    enemyStats[i].characterParameters.SPreg = enemyCharacters[i].currentCharacterStats.SPregen;
                    enemyStats[i].characterParameters.SPmovecost = enemyCharacters[i].currentCharacterStats.SPmoveCost;

                    enemyStats[i].weaponParameters.ATK = enemyCharacters[i].weaponParameters.ATK;
                    enemyStats[i].weaponParameters.ACC = enemyCharacters[i].weaponParameters.ACC;
                    enemyStats[i].weaponParameters.CRIT = enemyCharacters[i].weaponParameters.CRIT;
                    enemyStats[i].weaponParameters.SE = enemyCharacters[i].weaponParameters.SE;
                    enemyStats[i].weaponParameters.SEreg = enemyCharacters[i].weaponParameters.SEreg;
                    enemyStats[i].weaponParameters.SEdec = enemyCharacters[i].weaponParameters.SEdec;

                    enemyStats[i].skillSet = new(enemyCharacters[i].skillSet);
                }
            }
        }
    }
}
