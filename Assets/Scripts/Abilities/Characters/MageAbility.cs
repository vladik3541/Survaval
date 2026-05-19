using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Mage auto-attack: launches homing magic orbs at the nearest enemy(ies).
/// Unique among abilities in that its projectiles actively track their targets.
/// All values driven by AbilityData — no hardcoded numbers.
/// </summary>
public class MageAbility : BaseAbility
{
    // ── Detection ─────────────────────────────────────────────────────────────
    [Header("Detection")]
    [SerializeField] private float     detectionRadius = 20f;
    [SerializeField] private LayerMask enemyLayer;

    // ── Upgrade State ──────────────────────────────────────────────────────────
    [Header("Upgrade State (read-only)")]
    [SerializeField] private int   orbCount   = 1;
    [SerializeField] private bool  hasAOE     = false;
    [SerializeField] private float aoeRadius  = 3f;
    [SerializeField] private bool  hasChain   = false;

    // ── Fire Logic ────────────────────────────────────────────────────────────

    /// <summary>
    /// Finds the N nearest enemies and fires one homing orb per target.
    /// Orb count scales with level (1 / 2 / 3).
    /// </summary>
    protected override void Fire()
    {
        if (Data == null || Data.projectilePrefab == null)
        {
            Debug.LogWarning("[MageAbility] Data або projectilePrefab = null", this);
            return;
        }

        if (enemyLayer.value == 0)
            Debug.LogWarning("[MageAbility] enemyLayer = Nothing! Виставте шар 'Enemy' у Inspector.", this);

        List<Transform> targets = FindNearestEnemies(orbCount);

        if (targets.Count == 0)
        {
            Debug.Log($"[MageAbility] Ворогів у радіусі {detectionRadius}м немає. LayerMask={enemyLayer.value}", this);
            return;
        }

        Debug.Log($"[MageAbility] Знайдено {targets.Count} цілей, спавн {orbCount} орб(и)", this);
        foreach (var target in targets)
            SpawnOrb(target);
    }

    // ── Upgrade Callbacks ──────────────────────────────────────────────────────

    /// <summary>
    /// Apply per-level stat changes.
    /// Level 2: 2 orbs  |  Level 3: AOE explosion  |
    /// Level 4: chain   |  Level 5: 3 orbs + wider AOE
    /// </summary>
    public override void OnLevelUp(int newLevel)
    {
        switch (newLevel)
        {
            case 2: orbCount = 2;                        break;
            case 3: hasAOE   = true;                     break;
            case 4: hasChain = true;                     break;
            case 5: orbCount = 3;
                    aoeRadius *= 1.5f;                   break;
        }
    }

    // ── Private Helpers ───────────────────────────────────────────────────────
    private List<Transform> FindNearestEnemies(int maxCount)
    {
        Collider[]      hits   = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
        List<Transform> result = new List<Transform>(maxCount);

        // Sort ascending by distance, take the closest `maxCount`
        System.Array.Sort(hits, (a, b) =>
            Vector3.SqrMagnitude(transform.position - a.transform.position)
            .CompareTo(Vector3.SqrMagnitude(transform.position - b.transform.position)));

        for (int i = 0; i < Mathf.Min(maxCount, hits.Length); i++)
            result.Add(hits[i].transform);

        return result;
    }

    private void SpawnOrb(Transform target)
    {
        var poolService = ServiceLocator.Get<PoolService>();
        if (poolService == null)
        {
            Debug.LogError("[MageAbility] PoolService не знайдено у ServiceLocator!", this);
            return;
        }

        Vector3    spawnPos = transform.position + spawnOffset;
        GameObject orbObj   = poolService.Get(Data.poolName, spawnPos, Quaternion.identity);
        if (orbObj == null)
        {
            Debug.LogError($"[MageAbility] PoolService.Get повернув null. Пул={Data.poolName}", this);
            return;
        }

        MagicOrbProjectile orb = orbObj.GetComponent<MagicOrbProjectile>();
        if (orb == null)
        {
            Debug.LogError($"[MageAbility] Компонент MagicOrbProjectile відсутній на {orbObj.name}", this);
            return;
        }

        orb.SetTarget(target);
        orb.SetAOE(hasAOE, aoeRadius);
        orb.SetChain(hasChain);
        orb.Init(Data.baseDamage, Data.baseProjectileSpeed, Data.baseRange, Vector3.zero);
        AudioManager.Instance.PlayMageProjectile();
        Debug.Log($"[MageAbility] Орб спавнено з {spawnPos} → ціль: {target.name}", this);
    }
}
