using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Stationary hitbox effect for the Warrior.
/// Does NOT travel — it scales up via DOTween, lingers for a short duration, then returns to pool.
/// Can hit multiple enemies per swing (tracked via HashSet to prevent double-damage).
/// Supports knockback (level 3+) and double-size scaling (level 5+).
/// </summary>
public class SwordSlashProjectile : BaseProjectile
{
    // ── Visual Settings ──────────────────────────────────────────────────────
    [Header("Slash Timing")]
    [SerializeField] private float scaleUpDuration = 0.15f;
    [SerializeField] private float lingerDuration  = 0.20f;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 8f;

    // ── State ────────────────────────────────────────────────────────────────
    private bool hasKnockback;
    private bool doubleSize;

    private readonly HashSet<EnemyHealth> hitEnemies = new HashSet<EnemyHealth>();
    private Tween activeTween;

    // ── Flags set by WarriorAbility ──────────────────────────────────────────

    /// <summary>Toggle knockback on hit (Warrior level 3+).</summary>
    public void SetKnockback(bool value) => hasKnockback = value;

    /// <summary>Toggle double-size scaling (Warrior level 5+). Must be set before Init.</summary>
    public void SetDoubleSize(bool value) => doubleSize = value;

    // ── Pool Lifecycle ───────────────────────────────────────────────────────
    private void OnDisable()
    {
        // Kill any in-flight tweens so they don't fire callbacks on a dead/recycled instance
        activeTween?.Kill();
        DOTween.Kill(transform);
    }

    // ── Init ──────────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void Init(float dmg, float spd, float rng, Vector3 direction)
    {
        base.Init(dmg, spd, rng, direction);
        hitEnemies.Clear();

        // Kill any leftover tweens from a previous pool cycle
        activeTween?.Kill();
        DOTween.Kill(transform);

        // Determine scale targets
        float baseScale   = doubleSize ? 0.6f  : 0.3f;
        float targetScale = doubleSize ? 2.0f  : 1.0f;

        transform.localScale = Vector3.one * baseScale;

        // Scale up, then wait, then return to pool
        activeTween = transform
            .DOScale(targetScale, scaleUpDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(false)           // respect Time.timeScale
            .OnComplete(() =>
            {
                activeTween = DOVirtual.DelayedCall(lingerDuration, ReturnToPool, false);
            });
    }

    // ── Movement (none) ──────────────────────────────────────────────────────

    /// <summary>Sword slashes are stationary — no movement logic required.</summary>
    public override void Move() { }

    // ── Collision ────────────────────────────────────────────────────────────

    protected override void OnTriggerEnter(Collider other)
    {
        // Do NOT short-circuit on hasHit — slashes linger and can hit multiple enemies
        if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy != null && !hitEnemies.Contains(enemy))
        {
            OnHitEnemy(enemy);
        }
    }

    // ── Hit Logic ─────────────────────────────────────────────────────────────

    /// <inheritdoc/>
    public override void OnHitEnemy(EnemyHealth enemy)
    {
        hitEnemies.Add(enemy);
        enemy.TakeDamage(damage);

        if (hasKnockback)
        {
            Rigidbody rb = enemy.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 knockDir = (enemy.transform.position - transform.position).normalized;
                knockDir.y = 0f;
                rb.AddForce(knockDir * knockbackForce, ForceMode.Impulse);
            }
        }
        // Do NOT return to pool here — the tween manages lifetime
    }
}
