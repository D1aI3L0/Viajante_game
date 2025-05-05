using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIController : MonoBehaviour
{
    public static BattleUIController Instance { get; private set; }

    [Header("Turn Panel")]
    public Button endTurnButton;
    public GameObject turnOrderPanel; // Панель очередности ходов (можно заменить на список/контейнер)

    [Header("Character Info Panel")]
    public Image healthBarFill;
    public TMP_Text hpText;
    public Image spBarFill;
    public TMP_Text spText;

    [Header("Movement  Panel")]
    public Button confirmPathButton;

    [Header("Subclass Panel")]
    public Image specialEnergyBarFill;
    public TMP_Text specialEnergyText; // если нужна числовая информация
    public Button[] skillButtons; // Предполагается, что элементов ровно 4
    public Button switchingWeaponsButton;

    // Ссылка на активного игрока
    private AllyBattleCharacter player;

    private void Awake()
    {
        // Устанавливаем синглтон
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }




    /// <summary>
    /// Вызывается для установки ссылки на активного персонажа.
    /// Эту функцию можно вызвать из PlayerTurnController при начале хода.
    /// </summary>
    public void SetPlayer(AllyBattleCharacter newPlayer)
    {
        // Если ранее уже был установлен игрок, отписываемся от его события
        if (player != null)
        {
            player.OnStatsChanged -= UpdateCharacterStats;
        }

        player = newPlayer;

        if (player != null)
        {
            // Подписываемся на событие изменений характеристик
            player.OnStatsChanged += UpdateCharacterStats;
            // Обновляем UI сразу после установки нового персонажа
            UpdateCharacterStats();
        }
    }


    /// <summary>
    /// Обновляет отображение характеристик персонажа.
    /// </summary>
    public void UpdateCharacterStats()
    {
        if (player == null)
            return;

        // Обновление HealthBar
        float hpRatio = (float)player.CurrentHP / player.maxHP;
        healthBarFill.fillAmount = hpRatio;
        hpText.text = player.CurrentHP + " / " + player.maxHP;

        // Обновление SPBar
        float spRatio = (float)player.CurrentSP / player.maxSP;
        spBarFill.fillAmount = spRatio;
        spText.text = player.CurrentSP + " / " + player.maxSP;

        // Обновление SpecialEnergyBar (если используется)
        // Например, если специальные очки зависят от currentSE
        float seRatio = (float)player.CurrentSE / 100f; // здесь 100f – максимальное значение, можно заменить на player.maxSE, если такое поле есть
        specialEnergyBarFill.fillAmount = seRatio;
        specialEnergyText.text = player.CurrentSE.ToString();
    }

    // Обработчики для кнопок навыков
    // Можно задать каждому навыковому индексу
    public void OnSkillButtonClicked(int skillIndex)
    {
        if (player != null)
        {
            // Предполагается, что у персонажа есть метод использования навыка
            //    player.UseSkill(skillIndex);
        }
    }

    public void OnSwitchWeaponsButtonClicked()
    {
        if (player != null)
        {
            //    player.SwitchWeapon(); // Реализуйте этот метод в AllyBattleCharacter
        }
    }

    // Обработчик для End Turn кнопки
    public void OnEndTurnButtonClicked()
    {
        // Вызывает соответствующий метод в PlayerTurnController
        PlayerTurnController.Instance.EndTurn();
    }

    public void OnConfirmPathClicked()
    {
        // Вызывает соответствующий метод в PlayerTurnController
        PlayerTurnController.Instance.ConfirmMove();
    }
}
