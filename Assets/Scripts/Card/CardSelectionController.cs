using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CardSelectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardSelectionPanel;
    [SerializeField] private List<GameObject> cards;

    [Header("Animation Settings")]
    [SerializeField] private float cardAppearDelay = 0.2f;
    [SerializeField] private float cardAnimDuration = 0.4f;
    [SerializeField] private Ease cardEase = Ease.OutBack;

    private float _previousTimeScale;

    private void Start()
    {
        LevelSystem.Instance.OnLevelUp += ShowCardSelection;
    }

    public void ShowCardSelection()
    {
        // Зберігаємо поточний timeScale та зупиняємо гру
        _previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // Вмикаємо панель
        cardSelectionPanel.SetActive(true);

        // Запускаємо анімацію карточок по черзі
        AnimateCardsSequentially();
    }

    public void HideCardSelection()
    {
        cardSelectionPanel.SetActive(false);
        Time.timeScale = _previousTimeScale;
    }

    private void AnimateCardsSequentially()
    {
        for (int i = 0; i < cards.Count; i++)
        {
            GameObject card = cards[i];

            // Ховаємо карточку перед анімацією
            card.transform.localScale = Vector3.zero;
            card.SetActive(true);

            // Анімація появи з затримкою (використовуємо unscaledTime бо timeScale = 0)
            card.transform
                .DOScale(Vector3.one, cardAnimDuration)
                .SetDelay(i * cardAppearDelay)
                .SetEase(cardEase)
                .SetUpdate(true); // SetUpdate(true) = ignoreTimeScale
        }
    }

    // Викликається при натисканні на карточку
    public void OnCardSelected(int cardIndex)
    {
        HideCardSelection();
        // Тут обробляєш логіку вибору карточки
        Debug.Log($"Вибрано карточку: {cardIndex}");
    }

    private void OnDestroy()
    {
        LevelSystem.Instance.OnLevelUp -= ShowCardSelection;
    }
}