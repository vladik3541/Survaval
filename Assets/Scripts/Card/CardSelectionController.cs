using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class CardSelectionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject cardSelectionPanel;
    [SerializeField] private List<SkillCardUI> cards;

    [Header("Animation Settings")]
    [SerializeField] private float cardAppearDelay = 0.2f;
    [SerializeField] private float cardAnimDuration = 0.4f;
    [SerializeField] private Ease cardEase = Ease.OutBack;

    private float _previousTimeScale;
    public event Action<int> OnCardChosen;

    private void Start()
    {
        LevelSystem.Instance.OnLevelUp += ShowCardSelection;
        for (int i = 0; i < cards.Count; i++)
        {
            int index = i;
            if (cards[index].TryGetComponent(out Button cardButton))
            {
                cardButton.onClick.AddListener(()=>OnCardSelected(index));
            }
        }
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
        // Отримай карточки від SkillManager і передай їх в UI
        var skills = SkillManager.Instance.GetRandomCards(cards.Count);
        // Тут заповнюй UI карточок даними зі skills (іконки, назви, описи)
        for (int i = 0; i < cards.Count; i++)
        {
            if (i < skills.Count)
                cards[i].Setup(skills[i]);
        }
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
            GameObject card = cards[i].gameObject;
            card.transform.localScale = Vector3.zero;
            card.SetActive(true);
            
            card.transform
                .DOScale(Vector3.one, cardAnimDuration)
                .SetDelay(i * cardAppearDelay)
                .SetEase(cardEase)
                .SetUpdate(true); // SetUpdate(true) = ignoreTimeScale
        }
    }
    
    public void OnCardSelected(int cardIndex)
    {
        HideCardSelection();
        OnCardChosen?.Invoke(cardIndex);
        Debug.Log($"Вибрано карточку: {cardIndex}");
    }

    private void OnDestroy()
    {
        LevelSystem.Instance.OnLevelUp -= ShowCardSelection;
    }
}