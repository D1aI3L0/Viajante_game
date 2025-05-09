using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemiesController : MonoBehaviour
{
    public static EnemiesController Instance;

    public HexGrid grid;
    public List<EnemyBattleCharacter> availableEnemies;
    private Dictionary<Squad, int> lastLiveTurns = new();
    
    public int miтEnemiesCount = 1, maxEnemiesCount = 4;
    public int minSquadsPerTurn = 3, maxSquadsPerTurn = 6;
    public int minGenerationRadius = 3, maxGenerationRadius = 10;
    private int nextSquadGenerationTurn;
    public int minTurnDelay = 2, maxTurnDelay = 5;
    public int minLiveTime = 3, maxLiveTime = 5;

    private void Awake()
    {
        Instance = this;
    }

    public void OnTurnEnd(int currentTurn)
    {
        foreach (Squad enemiesSquad in lastLiveTurns.Keys.ToList())
        {
            lastLiveTurns[enemiesSquad]--;
            if (lastLiveTurns[enemiesSquad] <= 0)
            {
                lastLiveTurns.Remove(enemiesSquad);
                enemiesSquad.Die();
            }
        }
        if (currentTurn == nextSquadGenerationTurn || currentTurn == 0)
        {
            GenerateEnemySquads();
            nextSquadGenerationTurn = currentTurn + UnityEngine.Random.Range(minTurnDelay, maxTurnDelay + 1);
        }
    }

    private void GenerateEnemySquads()
    {
        int squadsCount = UnityEngine.Random.Range(minSquadsPerTurn, maxSquadsPerTurn + 1);
        for (int i = 0; i < squadsCount; i++)
        {
            GenerateEnemySquad();
        }
    }

    private void GenerateEnemySquad()
    {
        HexCell location = grid.GetAvailableEnemyCell(minGenerationRadius, maxGenerationRadius);
        if (location)
        {
            List<EnemyCharacter> characters = new();
            int enemiesCount = UnityEngine.Random.Range(miтEnemiesCount, maxEnemiesCount + 1);

            if (availableEnemies != null && availableEnemies.Count > 0)
            {
                for (int i = 0; i < enemiesCount; i++)
                {
                    EnemyCharacter newEnemy = new();
                    newEnemy.Initialize(availableEnemies[UnityEngine.Random.Range(0, availableEnemies.Count)]);
                    characters.Add(newEnemy);
                }
            }

            Squad newSquad = grid.AddEnemySquad(location, Random.Range(0f, 360f), characters);

            int liveTime = UnityEngine.Random.Range(minLiveTime, maxLiveTime + 1);
            lastLiveTurns.Add(newSquad, liveTime);
        }
    }
}
