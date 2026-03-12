using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerUpSlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Посилання на елементи")]
    [SerializeField] private Image          iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image[]        levelPips;      // масив pip-зображень (макс рівень штук)
    [SerializeField] private Image          selectHighlight;
    [SerializeField] private Image          lockedOverlay;  // напівпрозорий оверлей якщо макс рівень

    [Header("Кольори")]
    [SerializeField] private Color pipActiveColor   = new Color(0.9f, 0.6f, 0.1f);
    [SerializeField] private Color pipInactiveColor = new Color(0.3f, 0.3f, 0.3f);
    [SerializeField] private Color maxLevelColor    = new Color(0.9f, 0.8f, 0.2f);

    // ── стан ─────────────────────────────────────────────────────────────────
    public PowerUpData Data { get; private set; }
    private PowerUpSelectionUI parentUI;

    // ── ініціалізація ─────────────────────────────────────────────────────────
    public void Init(PowerUpData data, PowerUpSelectionUI parent)
    {
        Data     = data;
        parentUI = parent;
        selectHighlight.gameObject.SetActive(false);
        Refresh();
    }

    public void Refresh()
    {
        if (Data == null) return;

        int currentLevel = PowerUpManager.Instance.GetLevel(Data.type);
        bool isMaxed     = currentLevel >= Data.maxLevel;

        // іконка і назва
        if (iconImage) iconImage.sprite = Data.icon;
        if (nameText)
        {
            nameText.text  = Data.displayName;
            nameText.color = isMaxed ? maxLevelColor : Color.white;
        }

        // піпи рівня
        for (int i = 0; i < levelPips.Length; i++)
        {
            if (i >= Data.maxLevel)
            {
                levelPips[i].gameObject.SetActive(false);
                continue;
            }
            levelPips[i].gameObject.SetActive(true);
            levelPips[i].color = i < currentLevel ? pipActiveColor : pipInactiveColor;
        }

        // затемнення при макс рівні
        if (lockedOverlay) lockedOverlay.gameObject.SetActive(isMaxed);
    }

    public void SetSelected(bool selected)
    {
        if (selectHighlight) selectHighlight.gameObject.SetActive(selected);
    }

    // ── кліки ────────────────────────────────────────────────────────────────
    public void OnPointerClick(PointerEventData _)  => parentUI?.SelectSlot(this);

    public void OnPointerEnter(PointerEventData _)
    {
    }

    public void OnPointerExit(PointerEventData _)   { /* можна прибрати hover */ }
}
