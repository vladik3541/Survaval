using UnityEngine;

/// <summary>
/// Fast, straight-line projectile for the Archer.
/// Rotates to face its direction of travel and supports pierce-through on higher levels.
/// Base damage: 35. Uses transform.Translate for movement (no physics drag/gravity).
/// </summary>
public class ArrowProjectile : BaseProjectile
{
    // ── State ────────────────────────────────────────────────────────────────
    private Vector3 moveDirection;
    private bool    canPierce;
    private int     pierceCount;

    [Header("Pierce Settings")]
    [SerializeField] private int maxPierceTargets = 2;

    // ── Public API ───────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void Init(float dmg, float spd, float rng, Vector3 direction)
    {
        base.Init(dmg, spd, rng, direction);
        moveDirection = direction.normalized;
        pierceCount   = 0;

        // Snap rotation to face direction immediately
        if (moveDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(moveDirection);
    }

    /// <summary>Enable or disable pierce-through behaviour (Archer level 4+).</summary>
    public void SetPierce(bool pierce) => canPierce = pierce;

    // ── Movement ─────────────────────────────────────────────────────────────

    /// <summary>Moves the arrow forward in local space and aligns rotation to travel direction.</summary>
    public override void Move()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }

    // ── Hit Logic ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnHitEnemy(EnemyHealth enemy)
    {
        enemy.TakeDamage(damage);

        if (canPierce && pierceCount < maxPierceTargets)
        {
            pierceCount++;
            // Arrow continues flying — do NOT return to pool
        }
        else
        {
            ReturnToPool();
        }
    }
}
