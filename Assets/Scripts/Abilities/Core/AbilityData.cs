using UnityEngine;

/// <summary>
/// ScriptableObject that holds all base stats and upgrade definitions for a single ability.
/// Create via: Assets → Create → Ability → AbilityData
/// </summary>
[CreateAssetMenu(fileName = "NewAbilityData", menuName = "Ability/AbilityData")]
public class AbilityData : ScriptableObject
{
    [Header("Identity")]
    public string abilityName;
    public Sprite icon;

    [Header("Base Stats")]
    public float baseDamage        = 20f;
    public float baseCooldown      = 1.5f;
    public float baseProjectileSpeed = 12f;
    public float baseRange         = 10f;

    [Header("Pool")]
    [Tooltip("Ім'я пулу в PoolService. Повинно збігатись з Poolable.PoolName на prefab-і.")]
    public PoolName poolName;
    public GameObject projectilePrefab;

    [Header("Level Upgrades")]
    [Tooltip("Index 0 = Level 2 upgrade, index 1 = Level 3, …up to index 3 = Level 5")]
    public AbilityUpgradeData[] levelUpgrades;
}
