using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroSelectionUI : MonoBehaviour
{
    [Header("Всі герої у грі")]
    [SerializeField] private List<HeroData> allHeroes = new();

    [Header("Сітка карточок")]
    [SerializeField] private Transform    gridParent;
    [SerializeField] private HeroSlotUI  slotPrefab;

    [Header("Інфо-панель")]
    [SerializeField] private Image             infoIcon;
    [SerializeField] private TextMeshProUGUI   infoName;
    [SerializeField] private TextMeshProUGUI   infoDescription;
    [SerializeField] private TextMeshProUGUI   infoCost;

    [Header("Кнопки дій")]
    [SerializeField] private Button          buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;
    [SerializeField] private Button          startButton;
    [SerializeField] private Button          backButton;

    [Header("Назва ігрової сцени")]
    [SerializeField] private string gameSceneName = "Game";

    // ── стан ─────────────────────────────────────────────────────────────────
    private readonly List<HeroSlotUI> slots = new();
    private HeroSlotUI selectedSlot;

    // ── Unity lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        buyButton?.onClick.AddListener(OnBuyClicked);
        startButton?.onClick.AddListener(OnStartClicked);
        backButton?.onClick.AddListener(OnBackClicked);
        startButton.gameObject.SetActive(false);
        
    }

    private void OnEnable()
    {
        BuildGrid();
        AutoSelectFirstUnlocked();
    }
    // ── побудова сітки ────────────────────────────────────────────────────────
    private void BuildGrid()
    {
        foreach (var s in slots) Destroy(s.gameObject);
        slots.Clear();
        selectedSlot = null;

        foreach (var data in allHeroes)
        {
            var slot = Instantiate(slotPrefab, gridParent);
            slot.Init(data, this);
            slots.Add(slot);
        }
    }

    private void AutoSelectFirstUnlocked()
    {
        foreach (var slot in slots)
        {
            if (slot.IsUnlocked())
            {
                SelectHero(slot);
                return;
            }
        }
        // якщо жоден не розблокований — вибираємо перший
        if (slots.Count > 0) SelectHero(slots[0]);
    }

    // ── вибір героя ───────────────────────────────────────────────────────────
    public void SelectHero(HeroSlotUI slot)
    {
        if (selectedSlot == slot) return;

        selectedSlot?.SetSelected(false);
        selectedSlot = slot;
        selectedSlot.SetSelected(true);

        UpdateInfoPanel(slot.Data);
    }

    private void UpdateInfoPanel(HeroData data)
    {
        if (data == null) return;

        bool unlocked = selectedSlot != null && selectedSlot.IsUnlocked();
        int  cost     = data.price;

        ServiceLocator.TryGet<SaveService>(out var save);
        bool canAfford = MoneyMenuManager.Instance.Coins >= cost;

        if (infoIcon)        infoIcon.sprite      = data.icon;
        if (infoName)        infoName.text         = data.displayName;
        if (infoDescription) infoDescription.text  = data.description;
        if (infoCost)        infoCost.text         = unlocked ? "Unlock" : $"Price: ${cost}";

        // кнопка «Купити» — тільки для заблокованих
        if (buyButton)
        {
            buyButton.gameObject.SetActive(!unlocked);
            buyButton.interactable = canAfford;
        }
        if (buyButtonText) buyButtonText.text = canAfford ? $"Buy (${cost})" : $"No money (${cost})";

        // кнопка «Старт» — тільки якщо вибраний герой розблокований
        if (startButton) startButton.gameObject.SetActive(unlocked);
    }

    // ── купівля героя ─────────────────────────────────────────────────────────
    private void OnBuyClicked()
    {
        if (selectedSlot == null) return;
        if (selectedSlot.IsUnlocked()) return;

        var data = selectedSlot.Data;
        if (!ServiceLocator.TryGet<SaveService>(out var save)) return;

        if (!MoneyMenuManager.Instance.EnoughCoins(data.price))
        {
            Debug.Log($"[HeroSelection] No money {data.displayName}");
            return;
        }

        save.UnlockHero(data.heroId);
        // PowerUpManager.coins синхронізується автоматично через SaveService.OnCoinsChanged
        MoneyMenuManager.Instance.RemoveCoins(data.price);
        selectedSlot.Refresh();
        UpdateInfoPanel(data);

        Debug.Log($"[HeroSelection] Buing hero: {data.displayName}. Залишок: ${save.Coins}");
    }

    // ── старт гри ─────────────────────────────────────────────────────────────
    private void OnStartClicked()
    {
        if (selectedSlot == null || !selectedSlot.IsUnlocked()) return;

        // зберігаємо вибраного героя в PlayerPrefs для передачі в ігрову сцену
        PlayerPrefs.SetString("SelectedHeroId", selectedSlot.Data.heroId);
        PlayerPrefs.Save();

        Debug.Log($"[HeroSelection] Start hero with : {selectedSlot.Data.heroId}");

        var scene = ServiceLocator.Get<SceneService>();
        scene?.LoadScene(gameSceneName);
    }

    // ── назад ─────────────────────────────────────────────────────────────────
    private void OnBackClicked()
    {
        gameObject.SetActive(false);
    }
    
    // ── публічне API ──────────────────────────────────────────────────────────
    public void Show() => gameObject.SetActive(true);
    public void Hide() => gameObject.SetActive(false);
}
