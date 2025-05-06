using UnityEngine;

public class UnitStatManager : MonoBehaviour
{
    public static UnitStatManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Вызывается после окончания хода юнита.
    /// Здесь определяется, является ли юнит союзником или врагом, и вызывается соответствующая обработка.
    /// </summary>
    /// <param name="unit">Юнит, чей ход завершён</param>
    public void ProcessEndOfTurn(BattleEntity unit)
    {
        if (unit == null)
            return;

        if (!unit.IsEnemy)
        {
            ProcessAllyRegeneration(unit as AllyBattleCharacter);
        }
        else
        {
            ProcessEnemyRegeneration(unit as EnemyBattleCharacter);
        }
    }

    /// <summary>
    /// Обработка регенерации для союзных персонажей, учитывая наличие двух оружий и особенностей SE.
    /// </summary>
    /// <param name="ally">Союзный юнит</param>
    private void ProcessAllyRegeneration(AllyBattleCharacter ally)
    {
        if (ally == null)
            return;

        // Восполнение SP: увеличиваем на SPreg, но не выше максимума.
        ally.CurrentSP = Mathf.Min(ally.CurrentSP + ally.SPreg, ally.maxSP);

        // Обновление SE для первого оружия
        ally.currentSEValues[0] = Mathf.Clamp(
            ally.currentSEValues[0] + ally.weaponParameters[0].SEreg - ally.weaponParameters[0].SEdec,
            0,
            ally.weaponParameters[0].SE);

        // Обновление SE для второго оружия
        ally.currentSEValues[1] = Mathf.Clamp(
            ally.currentSEValues[1] + ally.weaponParameters[1].SEreg - ally.weaponParameters[1].SEdec,
            0,
            ally.weaponParameters[1].SE);

        // Если активная стойка – та, значение которой только что обновили, можно вызвать событие для обновления UI:
        ally.RaiseOnStatsChanged();
    }


    /// <summary>
    /// Обработка регенерации для вражеских юнитов.
    /// У врагов, как правило, нет SE, поэтому можно обработать только SP (и при необходимости другие характеристики).
    /// </summary>
    /// <param name="enemy">Вражеский юнит</param>
    private void ProcessEnemyRegeneration(EnemyBattleCharacter enemy)
    {
        if (enemy == null)
            return;

        // Восполнение SP для врагов
        enemy.CurrentSP = Mathf.Min(enemy.CurrentSP + enemy.SPreg, enemy.maxSP);

        // Если в будущем добавятся дополнительные характеристики для врагов (например, эффект дебаффа на атаку),
        // их можно обработать здесь.
    }
}
