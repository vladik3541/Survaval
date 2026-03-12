using UnityEngine;

/// <summary>
/// ScriptableObject — дані одного PowerUp.
/// Створюється через: Assets → Create → PowerUp → PowerUpData
/// </summary>
[CreateAssetMenu(fileName = "NewPowerUp", menuName = "PowerUp/PowerUpData")]
public class PowerUpData : ScriptableObject
{
    [Header("Основна інфо")]
    public string displayName = "PowerUp";
    [TextArea] public string description = "Raises something by X% per level.";
    public Sprite icon;

    [Header("Прокачка")]
    public int maxLevel = 5;
    public int costPerLevel = 200;          // базова ціна (можна масштабувати)

    [Header("Статистика")]
    public PowerUpType type = PowerUpType.Might;
    public float valuePerLevel = 0.05f;     // наприклад 0.05 = +5% за рівень

    // ── зручні утиліти ──────────────────────────────────────────────────────

    public string GetDescription(int currentLevel)
    {
        float total = valuePerLevel * maxLevel * 100f;
        float current = valuePerLevel * currentLevel * 100f;
        return $"{description}\nРівень {currentLevel}/{maxLevel}  (+{current:0}% / max +{total:0}%)";
    }

    public int GetCostForLevel(int nextLevel)
    {
        return costPerLevel * nextLevel;    // ціна зростає з кожним рівнем
    }
}

public enum PowerUpType
{
    Might,
    MaxHealth,
    Armor,
    Amount,
    Cooldown,
    Area,
    Velocity,
    Duration,
    Speed,
    Magnet,
    Luck,
    Growth
}
