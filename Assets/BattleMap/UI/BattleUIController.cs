using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUIController : MonoBehaviour
{
    [Header("Turn Panel")]
    public Button endTurnButton;
    public GameObject turnOrderPanel; // Панель очередности ходов (можно заменить на список/контейнер)

    [Header("Character Info Panel")]
    public Image healthBarFill;
    public TMP_Text hpText;
    public Image spBarFill;
    public TMP_Text spText;

    [Header("Subclass Panel")]
    public Image specialEnergyBarFill;
    public TMP_Text specialEnergyText; // если нужна числовая информация
    public Button[] skillButtons; // Предполагается, что элементов ровно 4
    public Button switchingWeaponsButton;

    // Ссылка на активного игрока
    private AllyBattleCharacter player;

    /// <summary>
    /// Вызывается для установки ссылки на активного персонажа.
    /// Эту функцию можно вызвать из PlayerTurnController при начале хода.
    /// </summary>
    public void SetPlayer(AllyBattleCharacter newPlayer)
    {
        player = newPlayer;
        UpdateCharacterStats();
    }

    /// <summary>
    /// Обновляет отображение характеристик персонажа.
    /// Можно вызывать в Update(), либо подписаться на изменения у персонажа.
    /// </summary>
    public void UpdateCharacterStats()
    {
        if (player == null)
            return;

        // Обновление HealthBar
        float hpRatio = (float)player.currentHP / player.maxHP;
        healthBarFill.fillAmount = hpRatio;
        hpText.text = player.currentHP + " / " + player.maxHP;

        // Обновление SPBar
        float spRatio = (float)player.currentSP / player.maxSP;
        spBarFill.fillAmount = spRatio;
        spText.text = player.currentSP + " / " + player.maxSP;

        // Обновление SpecialEnergyBar (если используется)
        // Например, если специальные очки зависят от currentSE
        float seRatio = (float)player.CurrentSE / 100f; // здесь 100f – максимальное значение, можно заменить на player.maxSE, если такое поле есть
        specialEnergyBarFill.fillAmount = seRatio;
        specialEnergyText.text = player.CurrentSE.ToString();
    }

    private void Update()
    {
        // Здесь можно обновлять данные каждый кадр, либо переключиться на события
        UpdateCharacterStats();
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
        // Можно вызвать соответствующий метод в BattleManager или TurnManager
        BattleManager.Instance.OnTurnComplete();
    }
}
