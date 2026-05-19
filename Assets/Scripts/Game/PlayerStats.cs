using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float DamageMultiplier { get; private set; } = 1f;
    public float Armor { get; private set; } = 0f;

    private HeroData _hero;
    private PlayerHealth _health;
    private PlayerMovement _movement;
    private ItemCollector _collector;

    // Броня, накопичена під час поточного раню (пасивки при левел-апі)
    private float _runtimeArmorBonus = 0f;

    public void Initialize(HeroData hero)
    {
        _hero = hero;
        _health = GetComponent<PlayerHealth>();
        _movement = GetComponent<PlayerMovement>();
        _collector = GetComponent<ItemCollector>();

        _runtimeArmorBonus = 0f; // скидаємо при кожному новому раню

        if (PowerUpManager.Instance != null)
            PowerUpManager.Instance.OnPowerUpPurchased += OnPowerUpPurchased;

        ApplyAll();
    }

    private void OnDestroy()
    {
        if (PowerUpManager.Instance != null)
            PowerUpManager.Instance.OnPowerUpPurchased -= OnPowerUpPurchased;
    }

    private void OnPowerUpPurchased(PowerUpData _, int __) => ApplyAll();

    private void ApplyAll()
    {
        if (_hero == null) return;
        var pm = PowerUpManager.Instance;

        float newMaxHp = _hero.baseHealth * (1f + (pm?.MaxHealthBonus ?? 0f));
        _health?.SetMaxHealth(newMaxHp);

        float newSpeed = _hero.baseSpeed * (pm?.SpeedMultiplier ?? 1f);
        _movement?.SetSpeed(newSpeed);

        float newRadius = _hero.basePickupRadius + (pm?.MagnetRange ?? 0f);
        _collector?.SetRadius(newRadius);

        DamageMultiplier = _hero.baseDamageMultiplier * (pm?.MightMultiplier ?? 1f);

        // ── Броня: base + меню + рантайм ─────────────────────────────────────
        Armor = _hero.baseArmor
              + (pm?.ArmorBonus ?? 0f)   // прокачка в головному меню
              + _runtimeArmorBonus;       // пасивки під час гри
    }

    /// <summary>
    /// Додає броню під час гри (левел-ап / пасивка).
    /// Після додавання одразу перераховує Armor.
    /// </summary>
    public void AddRuntimeArmor(float amount)
    {
        _runtimeArmorBonus += amount;
        ApplyAll();
    }

    public void ApplyPassive(PassiveSkillData data)
    {
        switch (data.skillName)
        {
            case PassiveSkillName.armor:
                AddRuntimeArmor(data.value); // наприклад data.value = 5f
                break;

        }
    }
}