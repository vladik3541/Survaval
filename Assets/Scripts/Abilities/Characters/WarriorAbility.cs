using UnityEngine;

/// <summary>
/// Warrior auto-attack: spawns sword slash hitbox effects in the player's movement direction.
/// Short range, wide hitbox, fast cooldown, multiple slashes at higher levels.
/// All values driven by AbilityData — no hardcoded numbers.
/// </summary>
public class WarriorAbility : BaseAbility
{
    // ── References ────────────────────────────────────────────────────────────
    [Header("References")]
    [Tooltip("Player Rigidbody used to read movement direction. Auto-found in parent if left empty.")]
    [SerializeField] private Rigidbody playerRigidbody;

    // ── Upgrade State ─────────────────────────────────────────────────────────
    [Header("Upgrade State (read-only)")]
    [SerializeField] private int   slashCount       = 1;
    [SerializeField] private float damageMultiplier = 1f;
    [SerializeField] private bool  hasKnockback     = false;
    [SerializeField] private bool  spin360          = false;
    [SerializeField] private bool  doubleSize       = false;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    protected override void Awake()
    {
        base.Awake();
        if (playerRigidbody == null)
            playerRigidbody = GetComponentInParent<Rigidbody>();
    }

    // ── Fire Logic ────────────────────────────────────────────────────────────

    /// <summary>
    /// Spawns sword slash hitboxes around the player.
    /// Level 4+ fires a full 360° ring of 8 slashes; otherwise fires 1 or 3 directional slashes.
    /// </summary>
    protected override void Fire()
    {
        if (Data == null || Data.projectilePrefab == null) return;

        if (spin360)
        {
            FireCircle(8);
            return;
        }

        Vector3 baseDir = GetMovementDirection();
        FireDirectional(baseDir);
    }

    // ── Upgrade Callbacks ─────────────────────────────────────────────────────

    /// <summary>
    /// Apply per-level stat changes.
    /// Level 2: +2 angled slashes  |  Level 3: +30% dmg + knockback  |
    /// Level 4: 360° spin          |  Level 5: double slash size
    /// </summary>
    public override void OnLevelUp(int newLevel)
    {
        switch (newLevel)
        {
            case 2: slashCount = 3;                    break;
            case 3: damageMultiplier *= 1.30f;
                    hasKnockback = true;               break;
            case 4: spin360 = true;                    break;
            case 5: doubleSize = true;                 break;
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────
    private Vector3 GetMovementDirection()
    {
        if (playerRigidbody != null)
        {
            // Unity 2019–2023: velocity  |  Unity 6+: linearVelocity
            // Change '.velocity' to '.linearVelocity' if building on Unity 6+
            Vector3 vel = playerRigidbody.velocity;
            vel.y = 0f;
            if (vel.sqrMagnitude > 0.01f)
                return vel.normalized;
        }
        return transform.forward;
    }

    private void FireDirectional(Vector3 baseDir)
    {
        SpawnSlash(baseDir);

        if (slashCount >= 3)
        {
            SpawnSlash(Quaternion.Euler(0f,  45f, 0f) * baseDir);
            SpawnSlash(Quaternion.Euler(0f, -45f, 0f) * baseDir);
        }
    }

    private void FireCircle(int count)
    {
        for (int i = 0; i < count; i++)
        {
            float   angle = (360f / count) * i;
            Vector3 dir   = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
            SpawnSlash(dir);
            AudioManager.Instance.PlaySwordSwing();
        }
    }

    private void SpawnSlash(Vector3 direction)
    {
        Vector3    spawnPos = transform.position + spawnOffset + direction * 1.5f;
        Quaternion rot      = Quaternion.LookRotation(direction);

        GameObject slashObj = ServiceLocator.Get<PoolService>()
            .Get(Data.poolName, spawnPos, rot);
        if (slashObj == null) return;

        SwordSlashProjectile slash = slashObj.GetComponent<SwordSlashProjectile>();
        if (slash == null) return;

        slash.SetKnockback(hasKnockback);
        slash.SetDoubleSize(doubleSize);
        slash.Init(
            Data.baseDamage * damageMultiplier,
            Data.baseProjectileSpeed,
            Data.baseRange,
            direction
        );
    }
}
