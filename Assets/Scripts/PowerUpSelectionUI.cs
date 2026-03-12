using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerUpSelectionUI : MonoBehaviour
{
    public static PowerUpSelectionUI Instance { get; private set; }

    [Header("Сітка")]
    [SerializeField] private Transform         gridParent;
    [SerializeField] private PowerUpSlotUI     slotPrefab;

    [Header("Інфо-панель (низ)")]
    [SerializeField] private Image             infoIcon;
    [SerializeField] private TextMeshProUGUI   infoName;
    [SerializeField] private TextMeshProUGUI   infoDescription;
    [SerializeField] private TextMeshProUGUI   infoCost;
    [SerializeField] private Button            buyButton;
    [SerializeField] private TextMeshProUGUI   buyButtonText;

    [Header("Анімація (опціонально)")]
    [SerializeField] private Animator          panelAnimator;
    private static readonly int ShowHash = Animator.StringToHash("Show");

    // ── стан ─────────────────────────────────────────────────────────────────
    private readonly List<PowerUpSlotUI> slots = new();
    private PowerUpSlotUI selectedSlot;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        buyButton?.onClick.AddListener(OnBuyClicked);
    }

    private void OnEnable()
    {
        if (PowerUpManager.Instance == null) return;

        PowerUpManager.Instance.OnPowerUpPurchased += HandlePurchased;
        BuildGrid();
        if (panelAnimator) panelAnimator.SetTrigger(ShowHash);
    }

    private void OnDisable()
    {
        if (PowerUpManager.Instance != null)
            PowerUpManager.Instance.OnPowerUpPurchased -= HandlePurchased;
        Time.timeScale = 1f;
    }

    // ── побудова сітки ────────────────────────────────────────────────────────
    private void BuildGrid()
    {
        // очищаємо старі слоти
        foreach (var s in slots) Destroy(s.gameObject);
        slots.Clear();
        selectedSlot = null;

        foreach (var data in PowerUpManager.Instance.AllPowerUps)
        {
            var slot = Instantiate(slotPrefab, gridParent);
            slot.Init(data, this);
            slots.Add(slot);
        }

        // вибираємо перший слот за замовчуванням
        if (slots.Count > 0) SelectSlot(slots[0]);
    }

    // ── вибір слоту ───────────────────────────────────────────────────────────
    public void SelectSlot(PowerUpSlotUI slot)
    {
        if (selectedSlot == slot) return;

        selectedSlot?.SetSelected(false);
        selectedSlot = slot;
        selectedSlot.SetSelected(true);

        UpdateInfoPanel(slot.Data);
    }

    private void UpdateInfoPanel(PowerUpData data)
    {
        if (data == null) return;

        int  currentLevel = PowerUpManager.Instance.GetLevel(data.type);
        bool isMaxed      = currentLevel >= data.maxLevel;
        int  cost         = isMaxed ? 0 : data.GetCostForLevel(currentLevel + 1);
        bool canBuy       = PowerUpManager.Instance.CanBuy(data);

        if (infoIcon)        infoIcon.sprite    = data.icon;
        if (infoName)        infoName.text       = data.displayName;
        if (infoDescription) infoDescription.text = data.GetDescription(currentLevel);
        if (infoCost)        infoCost.text       = isMaxed ? "MAX LEVEL" : $"$ {cost}";

        if (buyButton)       buyButton.interactable = canBuy;
        if (buyButtonText)   buyButtonText.text     = isMaxed ? "Max" : "Buy";
    }

    // ── купівля ───────────────────────────────────────────────────────────────
    private void OnBuyClicked()
    {
        if (selectedSlot == null) return;
        PowerUpManager.Instance.TryBuy(selectedSlot.Data);
    }

    private void HandlePurchased(PowerUpData data, int newLevel)
    {
        // оновлюємо всі слоти
        foreach (var s in slots) s.Refresh();
        // оновлюємо інфо-панель якщо це поточний вибраний
        if (selectedSlot != null && selectedSlot.Data == data)
            UpdateInfoPanel(data);
    }
    // ── відкрити/закрити ──────────────────────────────────────────────────────
    public void Show() => gameObject.SetActive(true);

    public void Hide() => gameObject.SetActive(false);

    /// <summary>
    /// Викликай ззовні (наприклад, при левел-апі персонажа):
    ///   PowerUpSelectionUI.Instance?.Show();
    /// або через подію.
    /// </summary>
}
