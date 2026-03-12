using UnityEngine;

/// <summary>
/// Центральний менеджер характеристик гравця.
/// Застосовує базові стати з HeroData і всі бонуси від PowerUpManager.
/// Викликай Initialize(hero) після спавну гравця.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    /// <summary>Фінальний множник пошкодження — читається системами зброї.</summary>
    public float DamageMultiplier { get; private set; } = 1f;

    private HeroData       _hero;
    private PlayerHealth   _health;
    private PlayerMovement _movement;
    private GemCollector   _collector;

    // ── Ініціалізація ─────────────────────────────────────────────────────────

    /// <summary>
    /// Встановлює базові стати з HeroData та підписується на зміни PowerUp.
    /// Викликається з GameEntryPoint одразу після спавну гравця.
    /// </summary>
    public void Initialize(HeroData hero)
    {
        _hero      = hero;
        _health    = GetComponent<PlayerHealth>();
        _movement  = GetComponent<PlayerMovement>();
        _collector = GetComponent<GemCollector>();

        if (PowerUpManager.Instance != null)
            PowerUpManager.Instance.OnPowerUpPurchased += OnPowerUpPurchased;

        ApplyAll();
    }

    private void OnDestroy()
    {
        if (PowerUpManager.Instance != null)
            PowerUpManager.Instance.OnPowerUpPurchased -= OnPowerUpPurchased;
    }

    // ── Оновлення стат ────────────────────────────────────────────────────────

    private void OnPowerUpPurchased(PowerUpData _, int __) => ApplyAll();

    private void ApplyAll()
    {
        if (_hero == null) return;

        var pm = PowerUpManager.Instance;

        // ── HP: base × (1 + бонус MaxHealth) ─────────────────────────────────
        float newMaxHp = _hero.baseHealth * (1f + (pm?.MaxHealthBonus ?? 0f));
        _health?.SetMaxHealth(newMaxHp);

        // ── Швидкість: base × SpeedMultiplier ────────────────────────────────
        float newSpeed = _hero.baseSpeed * (pm?.SpeedMultiplier ?? 1f);
        _movement?.SetSpeed(newSpeed);

        // ── Радіус підбору: base + плоский бонус Magnet ───────────────────────
        float newRadius = _hero.basePickupRadius + (pm?.MagnetRange ?? 0f);
        _collector?.SetRadius(newRadius);

        // ── Множник пошкодження: base × MightMultiplier ───────────────────────
        DamageMultiplier = _hero.baseDamageMultiplier * (pm?.MightMultiplier ?? 1f);
    }
}
