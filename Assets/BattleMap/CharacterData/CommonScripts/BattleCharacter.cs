using UnityEngine;

public class BattleCharacter : MonoBehaviour
{
    [Tooltip("Индекс, соответствующий позиции этого персонажа в массиве runtime-данных из CharacterDataTransferParameters.")]
    public int characterIndex = 0;
    
    // Тут будут сохранены скопированные runtime-данные для дальнейшего использования во время боя.
    [HideInInspector]
    public CharacterRuntimeParameters runtimeParameters;

    /// <summary>
    /// Инициализирует персонажа с данными из runtimeData.
    /// Её можно вызвать из BattleMapManager при создании персонажа.
    /// </summary>
    /// <param name="runtimeData">Данные персонажа, полученные из CharacterDataTransferParameters</param>
    public void Initialize(CharacterRuntimeParameters runtimeData)
    {
        if (runtimeData == null)
        {
            Debug.LogError("Runtime data is null для персонажа с индексом: " + characterIndex);
            return;
        }

        runtimeParameters = runtimeData;

        // Пример: Применение параметров к компоненту здоровья (если такой имеется).
        
        // Health healthComponent = GetComponent<Health>();
        // if (healthComponent != null)
        // {
        //     healthComponent.maxHP = runtimeParameters.maxHP;
        //     healthComponent.currentHP = runtimeParameters.currentHP;
        // }

        // Дополнительно можно скопировать другие параметры:
        // Например, скорость, характеристики оружия, информацию об умениях и т.д.
        // Пример:
        // Movement movementComponent = GetComponent<Movement>();
        // if(movementComponent != null)
        // {
        //     movementComponent.speed = runtimeParameters.SPD;
        // }

        Debug.Log("BattleCharacter с индексом " + characterIndex + " и классом " + runtimeParameters.characterClass.ToString() + " инициализирован.");
    }
}
