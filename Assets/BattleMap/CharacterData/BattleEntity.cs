using UnityEngine;

public class BattleEntity : MonoBehaviour
{
    // Параметры, связанные с ходом персонажа
    public bool isActiveTurn = false;
    public float turnGauge = 0f;

    public virtual bool IsEnemy => false; // По умолчанию юнит не является врагом.
    public virtual bool IsAlly => !IsEnemy; // Юнит считается союзником, если не является врагом.
    
    // Основные характеристики здоровья
    public int maxHP;
    public int currentHP;
    
    // Характеристики Защиты
    public int baseDEF;
    public int currentDEF;
    
    // Характеристики Уклонения
    public int baseEVA;
    public int currentEVA;
    
    // Характеристики Скорости
    public int baseSPD;
    public int currentSPD;

    /// <summary>
    /// Метод для получения урона. Принимает количество урона в качестве параметра.
    /// Если здоровье опускается ниже нуля, можно вызвать логику "смерти".
    /// </summary>
    /// <param name="amount">Количество урона.</param>
    public virtual void TakeDamage(float amount)
    {
        currentHP -= Mathf.RoundToInt(amount);
        Debug.Log($"{name} получает {amount:F1} урона. Остаток здоровья: {currentHP}");

        if (currentHP <= 0)
        {
            currentHP = 0;
            Debug.Log($"{name} уничтожен!");
            // Здесь можно добавить дополнительную логику, например, уничтожение объекта или запуск анимации.
            // Destroy(gameObject); // если нужно удалить объект из сцены.
        }
    }
}
