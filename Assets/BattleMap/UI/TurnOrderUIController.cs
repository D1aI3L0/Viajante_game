using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TurnOrderUIController : MonoBehaviour
{
    [Header("UI References")]
    // Контейнер для элементов очереди (дочерний объект TurnOrderPanel)
    public Transform turnOrderContainer;
    // Префаб для одного элемента очереди
    public GameObject turnOrderElementPrefab;
    // Интервал обновления UI, например, 0.2 секунды
    public float uiUpdateInterval = 0.2f;

    private void Start()
    {
        // Запускаем периодический апдейт очереди
        InvokeRepeating(nameof(UpdateTurnOrderDisplay), 0f, uiUpdateInterval);
    }

    /// <summary>
    /// Обновляет и сортирует отображение юнитов по turnGauge.
    /// Чем ниже показатель turnGauge, тем юнит ближе к ходу, соответственно, он должен быть ближе к левой части панели.
    /// </summary>
    public void UpdateTurnOrderDisplay()
    {
#if MIRROR
        List<BattleEntity> participants = new();
        foreach (uint id in TurnManager.Instance.SyncParticipants)
        {
            participants.Add(TurnManager.Instance.GetParticipantById(id));
        }
#else
        List<BattleEntity> participants = TurnManager.Instance.GetParticipants();
#endif
        List<BattleEntity> sortedUnits = participants
            .OrderBy(entity => entity.turnGauge)
            .ToList();

        // Очистка контейнера
        foreach (Transform child in turnOrderContainer)
        {
            Destroy(child.gameObject);
        }

        // Создание элементов очереди для каждого юнита
        foreach (BattleEntity entity in sortedUnits)
        {
            GameObject element = Instantiate(turnOrderElementPrefab, turnOrderContainer);
            // Предполагается, что в префабе есть компонент Image (для иконки)
            Image iconImage = element.GetComponentInChildren<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = entity.unitIcon;
            }

            // Если нужен текст для отображения turnGauge (например, ввиде числа)
            TMP_Text gaugeText = element.GetComponentInChildren<TMP_Text>();
            if (gaugeText != null)
            {
                gaugeText.text = entity.turnGauge.ToString("F1");
            }
        }
    }
}
