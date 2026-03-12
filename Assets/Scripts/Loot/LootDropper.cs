using UnityEngine;

/// <summary>
/// Статичний помічник для дропу лута у вказаній позиції.
/// Призначте prefab-и у Inspector через LootDropperConfig на сцені.
/// </summary>
public class LootDropper : MonoBehaviour
{
    public static LootDropper Instance { get; private set; }

    [Header("Prefab-и лута")]
    [SerializeField] private GameObject gemPrefab;
    [SerializeField] private GameObject healthPrefab;
    [SerializeField] private GameObject goldPrefab;

    [Header("Шанси дропу (0..1)")]
    [SerializeField] [Range(0f, 1f)] private float gemDropChance    = 0.50f;
    [SerializeField] [Range(0f, 1f)] private float healthDropChance = 0.25f;
    [SerializeField] [Range(0f, 1f)] private float goldDropChance   = 0.10f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Дропає лут у вказаній позиції. Якщо LootDropper відсутній на сцені — нічого не робить.
    /// </summary>
    public static void Drop(Vector3 position)
    {
        if (Instance == null) return;
        Instance.SpawnLoot(position);
    }

    private void SpawnLoot(Vector3 position)
    {
        if (gemPrefab    != null && Random.value <= gemDropChance)
            Instantiate(gemPrefab,    position, Quaternion.identity);

        if (healthPrefab != null && Random.value <= healthDropChance)
            Instantiate(healthPrefab, position, Quaternion.identity);

        if (goldPrefab   != null && Random.value <= goldDropChance)
            Instantiate(goldPrefab,   position, Quaternion.identity);
    }
}
