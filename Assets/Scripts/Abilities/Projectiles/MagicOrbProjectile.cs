using UnityEngine;
using DG.Tweening;

/// <summary>
/// Homing, spiralling orb projectile for the Mage.
/// Tracks its assigned target each frame. If the target dies it continues in the last known direction.
/// Supports AOE explosion on hit (level 3+) and chain-to-nearby-enemy on hit (level 4+).
/// Visual: sinusoidal spiral offset on the perpendicular axis + DOTween glow pulse on a child object.
/// </summary>
public class MagicOrbProjectile : BaseProjectile
{
    // ── Visual ───────────────────────────────────────────────────────────────
    [Header("Visual")]
    [SerializeField] private Transform visualChild;

    // ── State ─────────────────────────────────────────────────────────────────
    private Transform currentTarget;
    private Vector3   lastKnownDirection;

    private bool  hasAOE;
    private float aoeRadius;
    private bool  hasChain;
    private bool  chainUsed;        // prevent infinite chain loops

    private Tween glowTween;

    // ── Configuration injected by MageAbility ────────────────────────────────

    /// <summary>Set the homing target for this orb.</summary>
    public void SetTarget(Transform target)
    {
        currentTarget = target;
        if (target != null)
            lastKnownDirection = (target.position - transform.position).normalized;
    }

    /// <summary>Enable AOE explosion on hit and set its radius (Mage level 3+).</summary>
    public void SetAOE(bool enabled, float radius)
    {
        hasAOE    = enabled;
        aoeRadius = radius;
    }

    /// <summary>Enable chain-to-next-enemy behaviour (Mage level 4+).</summary>
    public void SetChain(bool enabled) => hasChain = enabled;

    // ── Pool Lifecycle ───────────────────────────────────────────────────────
    private void OnEnable()
    {
        StartGlowPulse();
    }

    private void OnDisable()
    {
        glowTween?.Kill();
        chainUsed = false;
    }

    // ── Init ─────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void Init(float dmg, float spd, float rng, Vector3 direction)
    {
        base.Init(dmg, spd, rng, direction);
        lastKnownDirection = Vector3.forward;
        chainUsed          = false;
    }

    // ── Movement ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Homing movement: steers toward the target each frame.
    /// Adds a sinusoidal spiral offset on the perpendicular axis for visual flair.
    /// Falls back to last known direction when the target is null (dead or despawned).
    /// </summary>
    public override void Move()
    {
        // Determine primary direction
        Vector3 moveDir;
        if (currentTarget != null)
        {
            moveDir            = (currentTarget.position - transform.position).normalized;
            lastKnownDirection = moveDir;
        }
        else
        {
            moveDir = lastKnownDirection;
        }

        moveDir.y = 0;
        // Perpendicular spiral oscillation
        Vector3 perp         = Vector3.Cross(moveDir, Vector3.up).normalized;

        transform.position += moveDir.normalized * speed * Time.deltaTime;

        // Face direction of travel
        if (moveDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(moveDir);
    }

    // ── Hit Logic ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnHitEnemy(EnemyHealth enemy)
    {
        enemy.TakeDamage(damage);

        if (hasAOE)
            ExplodeAOE(enemy);

        if (hasChain && !chainUsed)
        {
            bool chained = TryChain(enemy);
            if (chained) return; // orb continues toward chained target
        }

        ReturnToPool();
    }

    // ── Private Helpers ──────────────────────────────────────────────────────
    private void ExplodeAOE(EnemyHealth directHit)
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, aoeRadius, enemyLayer);
        foreach (var col in nearby)
        {
            EnemyHealth e = col.GetComponent<EnemyHealth>();
            if (e != null && e != directHit)
                e.TakeDamage(damage * 0.5f);
        }
    }

    private bool TryChain(EnemyHealth originalEnemy)
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, 6f, enemyLayer);
        foreach (var col in nearby)
        {
            EnemyHealth chainTarget = col.GetComponent<EnemyHealth>();
            if (chainTarget != null && chainTarget != originalEnemy)
            {
                chainUsed     = true;
                currentTarget = chainTarget.transform;
                hasHit        = false; // re-arm so collision can trigger again
                return true;
            }
        }
        return false;
    }

    private void StartGlowPulse()
    {
        if (visualChild == null) return;
        glowTween?.Kill();
        visualChild.localScale = Vector3.one * 0.9f;
        glowTween = visualChild
            .DOScale(1.1f, 0.4f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(false);
    }
}
