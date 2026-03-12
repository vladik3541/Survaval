using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// HP гравця: смерть → game-over, пошкодження → червона спалах-вінєтка.
/// </summary>
public class PlayerHealth : Health
{
    [Header("Vignette (UI Image — повноекранний overlay)")]
    [SerializeField] private Image vignetteImage;

    [Header("Аніматор гравця (для анімації смерті)")]
    [SerializeField] private Animator playerAnimator;

    // Константи
    private const float VignetteMaxAlpha    = 0.55f;
    private const float VignetteFadeDuration = 0.15f;
    private const float DeathFreezeDelay    = 1.5f;

    private static readonly int DieHash = Animator.StringToHash("Die");

    private bool isDead;

    // ── Стати ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Змінює максимальне HP зберігаючи відсоток поточного.
    /// Викликається PlayerStats при зміні HeroData або PowerUp.
    /// </summary>
    public void SetMaxHealth(float newMax)
    {
        if (newMax <= 0f) return;
        float ratio   = maxHealth > 0f ? currentHealth / maxHealth : 1f;
        maxHealth     = newMax;
        currentHealth = Mathf.Clamp(maxHealth * ratio, 0f, maxHealth);
    }

    /// <summary>
    /// Враховує броню з PowerUp Armor перед нанесенням пошкоджень.
    /// Armor = 0.2 → -20% вхідного пошкодження.
    /// </summary>
    public override void TakeDamage(float amount)
    {
        float armor   = PowerUpManager.Instance?.ArmorBonus ?? 0f;
        float reduced = amount * Mathf.Max(0f, 1f - armor);
        base.TakeDamage(reduced);
    }

    // ── Пошкодження → вінєтка ─────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();
        if (vignetteImage != null)
        {
            var c = vignetteImage.color;
            c.a = 0f;
            vignetteImage.color = c;
        }

        // Підписуємось на власну подію OnDamaged
        OnDamaged.AddListener(FlashVignette);
    }

    private void FlashVignette()
    {
        if (vignetteImage == null) return;

        vignetteImage.DOKill();
        vignetteImage.DOFade(VignetteMaxAlpha, VignetteFadeDuration)
                     .OnComplete(() => vignetteImage.DOFade(0f, VignetteFadeDuration * 2f));
    }

    // ── Смерть ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Грає анімацію смерті, зупиняє час через 1.5 с та викликає game-over.
    /// </summary>
    public override void Die()
    {
        if (isDead) return;
        isDead = true;

        // Зберігаємо зароблені монети до основного рахунку
        GoldSystem.Instance?.SaveSessionGold();

        OnDeath?.Invoke();

        if (playerAnimator != null)
            playerAnimator.SetTrigger(DieHash);

        StartCoroutine(FreezeAfterDelay());
    }

    private IEnumerator FreezeAfterDelay()
    {
        yield return new WaitForSecondsRealtime(DeathFreezeDelay);
        Time.timeScale = 0f;
        // GameManager або UI можна підписати на OnDeath і показати game-over панель
    }
}
