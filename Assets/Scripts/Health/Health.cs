using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Абстрактна база для будь-якого об'єкта з HP.
/// </summary>
public abstract class Health : MonoBehaviour
{
    [Header("Характеристики")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;

    [Header("Події")]
    public UnityEvent OnDeath;
    public UnityEvent OnDamaged;
    public UnityEvent OnHealed;

    /// <summary>Відсоток поточного HP (0..1) — для заповнення UI-бару.</summary>
    public float Percent => maxHealth > 0f ? Mathf.Clamp01(currentHealth / maxHealth) : 0f;

    public float CurrentHealth => currentHealth;
    public float MaxHealth     => maxHealth;

    protected virtual void Awake() => currentHealth = maxHealth;

    // ── публічний API ─────────────────────────────────────────────────────────

    /// <summary>Завдати damage. Якщо HP ≤ 0 — викликає Die().</summary>
    public virtual void TakeDamage(float amount)
    {
        if (amount <= 0f) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);
        OnDamaged?.Invoke();

        if (currentHealth <= 0f) Die();
    }

    /// <summary>Відновити HP, але не більше maxHealth.</summary>
    public virtual void Heal(float amount)
    {
        if (amount <= 0f) return;

        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealed?.Invoke();
    }

    /// <summary>Реакція на смерть — реалізується у підкласах.</summary>
    public abstract void Die();
}
