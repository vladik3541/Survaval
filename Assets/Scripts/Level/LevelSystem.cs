using System;
using UnityEngine;

/// <summary>
/// Singleton. Зберігає XP та рівень гравця протягом усього рану.
/// При левел-апі відкриває панель PowerUpSelectionUI.
/// </summary>
public class LevelSystem : MonoBehaviour
{
    public static LevelSystem Instance { get; private set; }

    // ── Стан ──────────────────────────────────────────────────────────────────

    [Header("Рівень")]
    [SerializeField] private int   currentLevel = 1;
    [SerializeField] private float currentXP;
    [SerializeField] private float xpToNextLevel;

    /// <summary>Викликається при кожному підвищенні рівня.</summary>
    public event Action<int, int> OnChangeExp;
    public event Action OnLevelUp;

    // Публічні властивості (тільки для читання)
    public int   CurrentLevel   => currentLevel;
    public float CurrentXP      => currentXP;
    public float XPToNextLevel  => xpToNextLevel;

    /// <summary>Прогрес XP (0..1) для UI-бару.</summary>
    public float XPPercent => xpToNextLevel > 0f ? Mathf.Clamp01(currentXP / xpToNextLevel) : 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        xpToNextLevel = CalcXPRequired(currentLevel);
    }

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>
    /// Додає XP та перевіряє левел-ап у циклі (можливо кілька за раз).
    /// </summary>
    public void AddXP(float amount)
    {
        if (amount <= 0f) return;

        currentXP += amount;

        // Цикл для можливих кількох левел-апів підряд
        while (currentXP >= xpToNextLevel)
        {
            currentXP    -= xpToNextLevel;
            currentLevel++;
            xpToNextLevel = CalcXPRequired(currentLevel);

            OnLevelUp?.Invoke();
            PowerUpSelectionUI.Instance?.Show();
        }
        OnChangeExp?.Invoke((int)currentXP, (int)xpToNextLevel);
    }

    // ── Допоміжне ─────────────────────────────────────────────────────────────

    /// <summary>XP, потрібна для переходу на наступний рівень.</summary>
    private static float CalcXPRequired(int level) => 100f * level * 1.15f;
}
