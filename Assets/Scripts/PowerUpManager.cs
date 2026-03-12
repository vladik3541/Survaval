using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public static PowerUpManager Instance { get; private set; }

    // ── події ────────────────────────────────────────────────────────────────
    public event Action<PowerUpData, int> OnPowerUpPurchased;   // дані, новий рівень

    // ── конфіг ───────────────────────────────────────────────────────────────
    [Header("Всі PowerUp у грі")]
    [SerializeField] private List<PowerUpData> allPowerUps = new();

    // ── стан ─────────────────────────────────────────────────────────────────
    private readonly Dictionary<PowerUpType, int> levels = new();

    // ── властивості-статистики ────────────────────────────────────────────────
    public float MightMultiplier    => 1f + GetBonus(PowerUpType.Might);
    public float MaxHealthBonus     => GetBonus(PowerUpType.MaxHealth);
    public float ArmorBonus         => GetBonus(PowerUpType.Armor);
    public int   AmountBonus        => Mathf.RoundToInt(GetBonus(PowerUpType.Amount));
    public float CooldownMultiplier => 1f - GetBonus(PowerUpType.Cooldown);
    public float AreaMultiplier     => 1f + GetBonus(PowerUpType.Area);
    public float VelocityMultiplier => 1f + GetBonus(PowerUpType.Velocity);
    public float DurationMultiplier => 1f + GetBonus(PowerUpType.Duration);
    public float SpeedMultiplier    => 1f + GetBonus(PowerUpType.Speed);
    public float MagnetRange        => GetBonus(PowerUpType.Magnet);
    public float LuckBonus          => GetBonus(PowerUpType.Luck);
    public float GrowthMultiplier   => 1f + GetBonus(PowerUpType.Growth);
    
    public IReadOnlyList<PowerUpData> AllPowerUps => allPowerUps;

    // ── ініціалізація ─────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ініціалізуємо рівні (0 за замовчуванням)
        foreach (var pu in allPowerUps)
            if (!levels.ContainsKey(pu.type))
                levels[pu.type] = 0;

        LoadFromSave();
        
    }
    
    // ── збереження / завантаження ─────────────────────────────────────────────

    private void LoadFromSave()
    {
        if (!ServiceLocator.TryGet<SaveService>(out var save))
        {
            return;
        }
        // Завантажуємо рівні прокачки
        foreach (var pu in allPowerUps)
        {
            int saved = save.GetPowerUpLevel(pu.type);
            levels[pu.type] = saved;
        }
    }
    private void SavePowerUpLevel(PowerUpType type)
    {
        if (ServiceLocator.TryGet<SaveService>(out var save))
            save.SetPowerUpLevel(type, levels[type]);
    }

    // ── публічне API ──────────────────────────────────────────────────────────

    public int GetLevel(PowerUpType type) =>
        levels.TryGetValue(type, out int lvl) ? lvl : 0;

    public bool CanBuy(PowerUpData data)
    {
        int lvl = GetLevel(data.type);
        if (lvl >= data.maxLevel) return false;
        return MoneyMenuManager.Instance.EnoughCoins(data.GetCostForLevel(lvl + 1));
    }

    public bool TryBuy(PowerUpData data)
    {
        if (!CanBuy(data)) return false;

        int nextLevel = GetLevel(data.type) + 1;
        int cost = data.GetCostForLevel(nextLevel);

        MoneyMenuManager.Instance.RemoveCoins(cost);
        levels[data.type] = nextLevel;
        
        SavePowerUpLevel(data.type);

        OnPowerUpPurchased?.Invoke(data, nextLevel);
        // OnCoinsChanged стріляє автоматично через SaveService.OnCoinsChanged → OnSaveCoinsChanged
        return true;
    }
    
    private float GetBonus(PowerUpType type)
    {
        if (!levels.TryGetValue(type, out int lvl)) return 0f;
        var data = allPowerUps.Find(p => p.type == type);
        return data != null ? data.valuePerLevel * lvl : 0f;
    }
    
}
