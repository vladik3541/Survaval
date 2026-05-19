using UnityEngine;

/// <summary>
/// Archer auto-attack: fires arrows at the nearest enemy within detection radius.
/// Upgrades unlock spread shot (level 2, 5) and pierce-through (level 4).
/// All values driven by AbilityData — no hardcoded numbers.
/// </summary>
public class ArcherAbility : BaseAbility
{
    // ── Detection ────────────────────────────────────────────────────────────
    [Header("Detection")]
    [SerializeField] private float       detectionRadius = 15f;
    [SerializeField] private LayerMask   enemyLayer;

    // ── Upgrade State ─────────────────────────────────────────────────────────
    [Header("Upgrade State (read-only)")]
    [SerializeField] private int   arrowCount        = 1;
    [SerializeField] private float spreadAngle       = 15f;
    [SerializeField] private float damageMultiplier  = 1f;
    [SerializeField] private bool  canPierce         = false;

    // ── Fire Logic ───────────────────────────────────────────────────────────

    /// <summary>
    /// Finds the nearest enemy and fires arrows toward it.
    /// Uses Physics.OverlapSphere (3D) for detection; arrow direction is set at spawn (not homing).
    /// </summary>
    protected override void Fire()
    {
        if (Data == null || Data.projectilePrefab == null)
        {
            Debug.LogWarning("[ArcherAbility] Fire: Data або projectilePrefab = null", this);
            return;
        }

        if (enemyLayer.value == 0)
            Debug.LogWarning("[ArcherAbility] enemyLayer = Nothing! Виставте шар 'Enemy' у Inspector.", this);

        Transform target = FindNearestEnemy();
        if (target == null)
        {
            Debug.Log($"[ArcherAbility] Ворогів у радіусі {detectionRadius}м немає. LayerMask={enemyLayer.value}", this);
            return;
        }

        Vector3 baseDir = (target.position - transform.position);
        baseDir.y = 0f;
        baseDir.Normalize();

        Debug.Log($"[ArcherAbility] Ціль: {target.name}, напрямок={baseDir}", this);
        SpawnArrowSpread(baseDir);
    }

    // ── Upgrade Callbacks ─────────────────────────────────────────────────────

    /// <summary>
    /// Apply per-level stat changes.
    /// Level 2: 3-arrow spread  |  Level 3: +20% damage  |
    /// Level 4: pierce 2 enemies  |  Level 5: 5-arrow spread
    /// </summary>
    public override void OnLevelUp(int newLevel)
    {
        switch (newLevel)
        {
            case 2: arrowCount = 3;                    break;
            case 3: damageMultiplier *= 1.20f;         break;
            case 4: canPierce = true;                  break;
            case 5: arrowCount = 5;                    break;
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────
    private void SpawnArrowSpread(Vector3 baseDir)
    {
        if (arrowCount == 1)
        {
            SpawnSingleArrow(baseDir);
            return;
        }

        // Distribute arrows symmetrically around baseDir
        float totalSpread = spreadAngle * (arrowCount - 1);
        float startAngle  = -totalSpread * 0.5f;

        for (int i = 0; i < arrowCount; i++)
        {
            float   angle = startAngle + spreadAngle * i;
            Vector3 dir   = Quaternion.Euler(0f, angle, 0f) * baseDir;
            SpawnSingleArrow(dir);
        }
    }

    private void SpawnSingleArrow(Vector3 direction)
    {
        var poolService = ServiceLocator.Get<PoolService>();
        if (poolService == null)
        {
            Debug.LogError("[ArcherAbility] PoolService не знайдено у ServiceLocator!", this);
            return;
        }

        Vector3    spawnPos = transform.position + spawnOffset;
        Quaternion arrowRot = Quaternion.LookRotation(direction);
        GameObject arrowObj = poolService.Get(Data.poolName, spawnPos, arrowRot);
        if (arrowObj == null)
        {
            Debug.LogError($"[ArcherAbility] PoolService.Get повернув null. Пул={Data.poolName}", this);
            return;
        }

        ArrowProjectile arrow = arrowObj.GetComponent<ArrowProjectile>();
        if (arrow == null)
        {
            Debug.LogError($"[ArcherAbility] Компонент ArrowProjectile відсутній на {arrowObj.name}", this);
            return;
        }

        arrow.SetPierce(canPierce);
        arrow.Init(
            Data.baseDamage * damageMultiplier,
            Data.baseProjectileSpeed,
            Data.baseRange,
            direction
        );
        AudioManager.Instance.PlayArrowShoot();
        Debug.Log($"[ArcherAbility] Стріла випущена з {spawnPos} → {direction}, dmg={Data.baseDamage * damageMultiplier}", this);
    }

    private Transform FindNearestEnemy()
    {
        Collider[] hits    = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        Transform  nearest = null;
        float      minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = hit.transform;
            }
        }

        return nearest;
    }
}
