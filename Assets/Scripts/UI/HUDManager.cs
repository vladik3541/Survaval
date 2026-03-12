using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HUD гри. Підписується на події систем і оновлює UI тільки при змінах.
/// Гравець передається ззовні через Initialize() після спавну.
/// </summary>
public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    // ── HP ────────────────────────────────────────────────────────────────────

    [Header("HP")]
    [SerializeField] private Image           healthFillImage;
    [SerializeField] private TextMeshProUGUI healthText;

    // ── XP / Рівень ───────────────────────────────────────────────────────────

    [Header("XP / Рівень")]
    [SerializeField] private Image           xpFillImage;
    [SerializeField] private TextMeshProUGUI levelText;

    // ── Золото ────────────────────────────────────────────────────────────────

    [Header("Золото")]
    [SerializeField] private TextMeshProUGUI goldText;

    // ── Кіли ─────────────────────────────────────────────────────────────────

    [Header("Кіли")]
    [SerializeField] private TextMeshProUGUI killCountText;

    // ── Таймер ────────────────────────────────────────────────────────────────

    [Header("Таймер")]
    [SerializeField] private TextMeshProUGUI timerText;

    // ── Внутрішній стан ───────────────────────────────────────────────────────

    private PlayerHealth _playerHealth;
    private float        _startTime;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _startTime = Time.time;

        // Підписуємось на системи, що вже існують (DDOL singletons)
        SubscribeToSystems();

        // Початковий стан UI
        RefreshGold(GoldSystem.Instance != null ? GoldSystem.Instance.SessionGold : 0);
        RefreshKills(ScoreManager.Instance != null ? ScoreManager.Instance.KillCount : 0);
    }

    private void OnDestroy()
    {
        UnsubscribeFromSystems();
        UnsubscribeFromPlayer();
    }

    private void Update()
    {
        UpdateTimer();
    }

    // ── Ініціалізація гравця ──────────────────────────────────────────────────

    /// <summary>
    /// Викликається GameEntryPoint після спавну гравця.
    /// Підписується на події HP та показує початкові значення.
    /// </summary>
    public void Initialize(PlayerHealth playerHealth)
    {
        UnsubscribeFromPlayer(); // на випадок повторного виклику

        _playerHealth = playerHealth;

        _playerHealth.OnDamaged.AddListener(RefreshHP);
        _playerHealth.OnHealed.AddListener(RefreshHP);
        _playerHealth.OnDeath.AddListener(OnPlayerDied);

        RefreshHP(); // одразу показуємо поточне HP
    }

    // ── Підписки на системи ───────────────────────────────────────────────────

    private void SubscribeToSystems()
    {
        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.OnChangeExp += RefreshXP;
            LevelSystem.Instance.OnLevelUp   += RefreshLevel;
            RefreshLevel();
        }

        if (GoldSystem.Instance != null)
            GoldSystem.Instance.OnGoldChanged += RefreshGold;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnKillCountChanged += RefreshKills;
    }

    private void UnsubscribeFromSystems()
    {
        if (LevelSystem.Instance != null)
        {
            LevelSystem.Instance.OnChangeExp -= RefreshXP;
            LevelSystem.Instance.OnLevelUp   -= RefreshLevel;
        }

        if (GoldSystem.Instance != null)
            GoldSystem.Instance.OnGoldChanged -= RefreshGold;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.OnKillCountChanged -= RefreshKills;
    }

    private void UnsubscribeFromPlayer()
    {
        if (_playerHealth == null) return;

        _playerHealth.OnDamaged.RemoveListener(RefreshHP);
        _playerHealth.OnHealed.RemoveListener(RefreshHP);
        _playerHealth.OnDeath.RemoveListener(OnPlayerDied);

        _playerHealth = null;
    }

    // ── Оновлення UI ─────────────────────────────────────────────────────────

    private void RefreshHP()
    {
        if (_playerHealth == null) return;

        if (healthFillImage != null)
            healthFillImage.fillAmount = _playerHealth.Percent;

        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(_playerHealth.CurrentHealth)} / {Mathf.CeilToInt(_playerHealth.MaxHealth)}";
    }

    private void RefreshXP(int currentXP, int maxXP)
    {
        if (xpFillImage != null)
            xpFillImage.fillAmount = maxXP > 0 ? (float)currentXP / maxXP : 0f;
    }

    private void RefreshLevel()
    {
        if (levelText != null && LevelSystem.Instance != null)
            levelText.text = $"LVL {LevelSystem.Instance.CurrentLevel}";
    }

    private void RefreshGold(int gold)
    {
        if (goldText != null)
            goldText.text = $"{gold}";
    }

    private void RefreshKills(int count)
    {
        if (killCountText != null)
            killCountText.text = $"{count}";
    }

    private void OnPlayerDied()
    {
        // Показуємо 0 HP при смерті
        if (healthFillImage != null) healthFillImage.fillAmount = 0f;
        if (healthText != null)      healthText.text = "0";
    }

    private void UpdateTimer()
    {
        float elapsed = Time.time - _startTime;
        int   minutes = Mathf.FloorToInt(elapsed / 60f);
        int   seconds = Mathf.FloorToInt(elapsed % 60f);

        if (timerText != null)
            timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
