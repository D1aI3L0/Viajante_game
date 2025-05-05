using UnityEngine;

public class BattleEntity : MonoBehaviour
{
    // Поле для хранения ссылки на клетку, в которой сейчас находится юнит
    public BattleCell currentCell { get; set; }

    // Ивент изменения характеристик
    public event System.Action OnStatsChanged;


    // Параметры, связанные с ходом персонажа
    public bool isActiveTurn = false;
    public float turnGauge = 0f;

    public virtual bool IsEnemy => false; // По умолчанию юнит не является врагом.
    public virtual bool IsAlly => !IsEnemy; // Юнит считается союзником, если не является врагом.

    // Основные характеристики здоровья
    public int maxHP;
    private int _currentHP;
    public int CurrentHP
    {
        get => _currentHP;
        set
        {
            if (_currentHP != value)
            {
                _currentHP = value;
                OnStatsChanged?.Invoke();
            }
        }
    }


    // Характеристики Защиты
    public int baseDEF;
    public int currentDEF;

    // Характеристики Уклонения
    public int baseEVA;
    public int currentEVA;

    // Характеристики Скорости
    public int baseSPD;
    public int currentSPD;

    // Основные характеристики SP
    public int maxSP;
    private int _currentSP;
    public int CurrentSP
    {
        get => _currentSP;
        set
        {
            if (_currentSP != value)
            {
                _currentSP = value;
                Debug.Log($"{name} Updated CurrentSP: {_currentSP}");
                OnStatsChanged?.Invoke();
            }
        }
    }
    public int SPreg;
    public int SPmovecost;

    /// <summary>
    /// Метод для получения урона. Принимает количество урона в качестве параметра.
    /// Если здоровье опускается ниже нуля, можно вызвать логику "смерти".
    /// </summary>
    /// <param name="amount">Количество урона.</param>
    public virtual void TakeDamage(float amount)
    {
        CurrentHP -= Mathf.RoundToInt(amount);
        Debug.Log($"{name} получает {amount:F1} урона. Остаток здоровья: {CurrentHP}");

        if (CurrentHP <= 0)
        {
            CurrentHP = 0;
            Debug.Log($"{name} уничтожен!");
            // Здесь можно добавить дополнительную логику, например, уничтожение объекта или запуск анимации.
            // Destroy(gameObject); // если нужно удалить объект из сцены.
        }
    }
}
