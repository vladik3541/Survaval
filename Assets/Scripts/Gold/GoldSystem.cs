using System;
using UnityEngine;

/// <summary>
/// Singleton. Зберігає золото поточного рану.
/// При смерті або виході — додає зароблене до SaveService.coins (основні монети меню).
/// </summary>
public class GoldSystem : MonoBehaviour
{
    public static GoldSystem Instance { get; private set; }

    [Header("Стан сесії")]
    [SerializeField] private int sessionGold;

    /// <summary>Золото, зароблене за поточний ран.</summary>
    public int SessionGold => sessionGold;

    /// <summary>Спрацьовує при кожній зміні золота в сесії. Передає нове значення.</summary>
    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnApplicationQuit()   => SaveSessionGold();
    private void OnApplicationPause(bool paused) { if (paused) SaveSessionGold(); }

    // ── API ───────────────────────────────────────────────────────────────────

    /// <summary>Додає золото до поточного рану та оновлює HUD.</summary>
    public void AddGold(int amount)
    {
        if (amount <= 0) return;
        sessionGold += amount;
        OnGoldChanged?.Invoke(sessionGold);
    }

    /// <summary>
    /// Переносить зароблене за ран золото до основних монет (SaveService.coins).
    /// Викликається при смерті гравця та при виході з гри.
    /// </summary>
    public void SaveSessionGold()
    {
        if (sessionGold <= 0) return;

        if (ServiceLocator.TryGet<SaveService>(out var save))
        {
            save.AddCoins(sessionGold);
            Debug.Log($"[GoldSystem] Збережено {sessionGold} монет до загального рахунку.");
        }
        else
        {
            Debug.LogWarning("[GoldSystem] SaveService недоступний — монети не збережено.");
        }

        sessionGold = 0;
        OnGoldChanged?.Invoke(sessionGold);
    }
}
