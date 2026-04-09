using UnityEngine;

/// <summary>
/// Abstract base for all character abilities.
/// Attach one concrete subclass per character. Fires automatically via cooldown cycle.
/// </summary>
public abstract class BaseAbility : MonoBehaviour, IUpgradeable
{
    // ── Data ────────────────────────────────────────────────────────────────
    [Header("Ability Data")]
    [SerializeField] protected AbilityData data;

    // ── Runtime State ───────────────────────────────────────────────────────
    [Header("Runtime (read-only in inspector)")]
    [SerializeField] private float cooldownTimer;
    [SerializeField] private int   currentLevel = 1;

    // ── Debug ────────────────────────────────────────────────────────────────
    private bool _loggedNoData;
    private int  _fireCount;

    // ── References ──────────────────────────────────────────────────────────
    protected Transform owner;

    // ── Spawn Offset ─────────────────────────────────────────────────────────
    [Header("Spawn")]
    [Tooltip("Зміщення відносно transform при спавні снаряда. Y=1 щоб не спавнити в підлогу.")]
    [SerializeField] protected Vector3 spawnOffset = new Vector3(0f, 1f, 0f);

    // ── IUpgradeable ────────────────────────────────────────────────────────
    /// <inheritdoc/>
    public int CurrentLevel => currentLevel;
    /// <inheritdoc/>
    public int MaxLevel => 5;

    // ── Public Properties ───────────────────────────────────────────────────
    /// <summary>Normalised cooldown progress (0 = just fired, 1 = ready). Use for UI fill bars.</summary>
    public float CooldownPercent =>
        data != null && data.baseCooldown > 0f
            ? 1f - Mathf.Clamp01(cooldownTimer / data.baseCooldown)
            : 1f;

    /// <summary>Read-only access to the ability's ScriptableObject data.</summary>
    public AbilityData Data => data;

    // ── Unity Lifecycle ─────────────────────────────────────────────────────
    protected virtual void Awake()
    {
        owner = transform;
    }

    protected virtual void Start()
    {
        if (data == null)
        {
            Debug.LogError($"[{GetType().Name}] AbilityData не призначено на {gameObject.name}! Стрільба не працюватиме.", this);
            return;
        }

        if (data.projectilePrefab == null)
            Debug.LogWarning($"[{GetType().Name}] AbilityData.projectilePrefab = null ({data.name})", this);

        cooldownTimer = data.baseCooldown;
        Debug.Log($"[{GetType().Name}] Ініціалізовано. Кулдаун={data.baseCooldown}s, Пул={data.poolName}, Prefab={data.projectilePrefab?.name ?? "NULL"}", this);
    }

    protected virtual void Update()
    {
        // Freeze everything while the game is paused (upgrade selection etc.)
        if (Time.timeScale <= 0f) return;

        cooldownTimer -= Time.deltaTime;
        TryFire();
    }

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Checks if the cooldown has elapsed. If so, fires the ability and resets the timer.
    /// Safe to call manually from external systems.
    /// </summary>
    public void TryFire()
    {
        if (cooldownTimer > 0f) return;

        if (data == null)
        {
            if (!_loggedNoData)
            {
                Debug.LogError($"[{GetType().Name}] TryFire: data == null на {gameObject.name}", this);
                _loggedNoData = true;
            }
            return;
        }

        _fireCount++;
        Debug.Log($"[{GetType().Name}] Fire #{_fireCount} (рівень={CurrentLevel})", this);
        Fire();
        cooldownTimer = data.baseCooldown;
    }

    /// <summary>
    /// Increases the ability level by 1 (capped at MaxLevel) and invokes OnLevelUp.
    /// Automatically applies the matching AbilityUpgradeData entry from AbilityData.levelUpgrades.
    /// </summary>
    public void Upgrade()
    {
        if (currentLevel >= MaxLevel) return;
        currentLevel++;
        OnLevelUp(currentLevel);

        // Apply ScriptableObject-driven upgrade data if available
        int upgradeIndex = currentLevel - 2; // level 2 → index 0
        if (data != null && data.levelUpgrades != null && upgradeIndex < data.levelUpgrades.Length)
        {
            ApplyUpgradeData(data.levelUpgrades[upgradeIndex]);
        }
    }

    // ── Abstract / Virtual ──────────────────────────────────────────────────

    /// <summary>Spawn or trigger the ability's main effect. Called by TryFire.</summary>
    protected abstract void Fire();

    /// <summary>
    /// Override per character to apply level-specific stat changes.
    /// Called automatically by Upgrade() after incrementing the level.
    /// </summary>
    /// <param name="newLevel">The level just reached (2–5).</param>
    public virtual void OnLevelUp(int newLevel) { }

    /// <summary>
    /// Override to handle ScriptableObject-driven upgrades (optional convenience hook).
    /// Base implementation is empty.
    /// </summary>
    protected virtual void ApplyUpgradeData(AbilityUpgradeData upgradeData) { }

    // ── Editor Helpers ──────────────────────────────────────────────────────

    /// <summary>Immediately triggers Fire() from the Unity editor context menu. For testing only.</summary>
    [ContextMenu("Test Fire")]
    public void TestFire() => Fire();

    [ContextMenu("Force Level Up")]
    private void ContextUpgrade() => Upgrade();
}
