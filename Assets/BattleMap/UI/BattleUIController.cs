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
    private AllyBattleCharacterSP player;

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
    public void SetPlayer(AllyBattleCharacterSP newPlayer)
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
            UpdateSkillButtons();
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

        // Обновление SpecialEnergyBar
        float seRatio = (float)player.CurrentSE / player.weaponParameters[player.currentWeaponIndex].SE;
        specialEnergyBarFill.fillAmount = seRatio;
        specialEnergyText.text = player.CurrentSE + " / " + player.weaponParameters[player.currentWeaponIndex].SE;
    }

    public void UpdateSkillButtons()
    {
        if (player == null || player.weaponSkillSelections == null || player.weaponSkills == null)
            return;

        // Получаем выбранный набор навыков для текущего оружия.
        WeaponSkillSelection currentSelection = player.weaponSkillSelections[player.currentWeaponIndex];
        WeaponSkillSet currentWeaponSkillSet = player.weaponSkills[player.currentWeaponIndex];

        // Обновляем кнопку базовой атаки (индекс 0) — базовая атака всегда берётся из текущего оружия по индексу 0.
        if (currentWeaponSkillSet.skills != null && currentWeaponSkillSet.skills.Length > 0)
        {
            SkillAsset basicSkill = currentWeaponSkillSet.skills[0];
            if (basicSkill != null)
            {
                // Обновляем изображение базовой атаки.
                Image baseBtnImage = skillButtons[0].GetComponentInChildren<Image>();
                if (baseBtnImage != null)
                    baseBtnImage.sprite = basicSkill.skillIcon;

                // Обновляем текст базовой атаки.
                TMP_Text baseBtnText = skillButtons[0].GetComponentInChildren<TMP_Text>();
                if (baseBtnText != null)
                    baseBtnText.text = basicSkill.skillName;
            }
            else
            {
                Debug.LogWarning("Базовая атака (SkillAsset, индекс 0) отсутствует.");
            }
        }
        else
        {
            Debug.LogWarning("Не найден массив навыков у текущего оружия.");
        }

        // Обновляем остальные кнопки навыков (начиная со второй кнопки, индекс 1)
        // При этом используем выбранные индексы из массива currentSelection.selectedSkillIndices.
        // Количество обновляемых кнопок будет равно минимальному значению: (skillButtons.Length - 1) и длине массива индексов.
        int count = Mathf.Min(skillButtons.Length - 1, currentSelection.selectedSkillIndices.Length);
        for (int i = 0; i < count; i++)
        {
            int selectedSkillIndex = currentSelection.selectedSkillIndices[i];
            if (selectedSkillIndex < 0 || selectedSkillIndex >= currentWeaponSkillSet.skills.Length)
            {
                Debug.LogWarning("Неверный индекс навыка: " + selectedSkillIndex);
                continue;
            }

            SkillAsset skillAsset = currentWeaponSkillSet.skills[selectedSkillIndex];
            if (skillAsset != null)
            {
                // Обновляем изображение кнопки (индекс в массиве кнопок = i+1).
                Image btnImage = skillButtons[i + 1].GetComponentInChildren<Image>();
                if (btnImage != null)
                    btnImage.sprite = skillAsset.skillIcon;

                // Обновляем текст кнопки (индекс в массиве кнопок = i+1).
                TMP_Text btnText = skillButtons[i + 1].GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                    btnText.text = skillAsset.skillName;
            }
            else
            {
                Debug.LogWarning("SkillAsset отсутствует для выбранного индекса: " + selectedSkillIndex);
            }
        }
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
            player.SwitchWeapon(); // Вызов смены стойки в AllyBattleCharacter
            UpdateSkillButtons(); // Обновление кнопок
            UpdateCharacterStats(); // Обновление параметров для обновления SE
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

    private void OnDestroy() // для отписки от событий при уничтожении
    {
        if (player != null)
        {
            player.OnStatsChanged -= UpdateCharacterStats;
        }
    }

}
