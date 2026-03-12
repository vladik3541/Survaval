using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Керує хвилями, спавном ворогів та ордами.
/// Використовує Object Pooling замість Instantiate/Destroy.
/// </summary>
public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    // ── Хвилі ────────────────────────────────────────────────────────────────

    [Header("Wave Config")]
    [Tooltip("Список хвиль, сортується автоматично за startTime")]
    [SerializeField] private List<WaveConfig> waves = new();

    [Tooltip("Поточний час гри в секундах (60 = 1 хвилина)")]
    [SerializeField] private float currentTime;

    [Tooltip("Активна хвиля (тільки для читання в Inspector)")]
    [SerializeField] private WaveConfig activeWave;

    // ── Орда ─────────────────────────────────────────────────────────────────

    [Header("Horde Events")]
    [SerializeField] private List<HordeConfig> hordeEvents = new();

    // ── Налаштування спавну ───────────────────────────────────────────────────

    [Header("Spawn Settings")]
    private Transform playerTransform;
    [Tooltip("Відстань від гравця до кола спавну")]
    [SerializeField] private float spawnRadius = 15f;
    [Tooltip("Випадкове відхилення радіусу (±)")]
    [SerializeField] private float spawnRadiusVariance = 3f;
    [SerializeField] private LayerMask groundLayer;

    // ── Пул і ліміти ─────────────────────────────────────────────────────────

    [Header("Pool & Limits")]
    [Tooltip("Кількість ворогів на екрані зараз (тільки для читання)")]
    [SerializeField] private int currentOnScreen;

    // ── Приватні поля ─────────────────────────────────────────────────────────

    private readonly Dictionary<EnemyData, Queue<GameObject>> _pools = new();
    private Camera   _mainCamera;
    private bool     _hordeActive;

    // ── Ініціалізація ─────────────────────────────────────────────────────────

    public void Initialize(Transform player)
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        playerTransform = player;
    }

    private void Start()
    {
        _mainCamera = Camera.main;

        // Якщо гравець не призначений — шукаємо по тегу
        if (playerTransform == null)
        {
            var player = GameObject.FindWithTag("Player");
            if (player != null) playerTransform = player.transform;
        }

        // Сортуємо хвилі за часом старту
        waves.Sort((a, b) => a.startTime.CompareTo(b.startTime));

        UpdateWave();
        StartCoroutine(TimerCoroutine());
        StartCoroutine(SpawnLoop());
    }

    // ── Таймер і хвилі ────────────────────────────────────────────────────────

    private IEnumerator TimerCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            currentTime += 1f;
            UpdateWave();
            CheckHordeEvents();
        }
    }

    /// <summary>
    /// Оновлює активну хвилю на основі поточного часу.
    /// Викликається автоматично кожну секунду і з тест-кнопок.
    /// </summary>
    public void UpdateWave()
    {
        WaveConfig candidate = null;

        foreach (var wave in waves)
        {
            if (currentTime >= wave.startTime)
                candidate = wave;
            else
                break;
        }

        if (activeWave != candidate)
        {
            activeWave = candidate;
            Debug.Log($"[SpawnManager] Нова хвиля: {(activeWave != null ? $"t={activeWave.startTime}s" : "none")}");
        }
    }

    private void CheckHordeEvents()
    {
        foreach (var horde in hordeEvents)
        {
            if (horde.triggered || currentTime < horde.triggerTime) continue;

            horde.triggered = true;

            if (!_hordeActive)
                StartCoroutine(SpawnHorde(horde));
        }
    }

    // ── Основний цикл спавну ──────────────────────────────────────────────────

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (activeWave == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            yield return new WaitForSeconds(activeWave.spawnInterval);

            if (currentOnScreen < activeWave.maxOnScreen)
                SpawnSingleEnemy(activeWave);
        }
    }

    private void SpawnSingleEnemy(WaveConfig wave)
    {
        var data = PickWeightedEnemy(wave.enemyPool);
        if (data == null) return;

        var pos = GetSpawnPosition();
        if (pos == null) return;

        var go = GetFromPool(data);
        go.transform.position = pos.Value;
        go.SetActive(true);

        var health = go.GetComponent<EnemyHealth>();
        health?.ApplyMultipliers(wave.hpMultiplier, wave.speedMultiplier);

        currentOnScreen++;
    }

    // ── Орда ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// Спавнить групу ворогів з одного напрямку (±15°) протягом 2 секунд.
    /// </summary>
    private IEnumerator SpawnHorde(HordeConfig config)
    {
        _hordeActive = true;

        float baseAngle = Random.Range(0f, 360f);
        float interval  = config.hordeCount > 0 ? 2f / config.hordeCount : 0.05f;

        Debug.Log($"[SpawnManager] Орда! {config.hordeCount}x {config.enemyType?.enemyName}, кут={baseAngle:F0}°");

        for (int i = 0; i < config.hordeCount; i++)
        {
            float angleRad = (baseAngle + Random.Range(-15f, 15f)) * Mathf.Deg2Rad;
            float dist     = spawnRadius + Random.Range(-spawnRadiusVariance, spawnRadiusVariance);

            var candidate = playerTransform != null
                ? playerTransform.position + new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * dist
                : Vector3.zero;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                var go = GetFromPool(config.enemyType);
                go.transform.position = hit.position;
                go.SetActive(true);

                var health = go.GetComponent<EnemyHealth>();
                if (health != null && activeWave != null)
                    health.ApplyMultipliers(activeWave.hpMultiplier, activeWave.speedMultiplier);

                currentOnScreen++;
            }

            yield return new WaitForSeconds(interval);
        }

        _hordeActive = false;
    }

    // ── Позиція спавну ────────────────────────────────────────────────────────

    /// <summary>
    /// Повертає валідну позицію на NavMesh поза полем зору камери.
    /// Повертає null якщо за 30 спроб не знайдено.
    /// </summary>
    public Vector3? GetSpawnPosition()
    {
        if (playerTransform == null) return null;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            float angleRad = Random.Range(0f, Mathf.PI * 2f);
            float dist     = spawnRadius + Random.Range(-spawnRadiusVariance, spawnRadiusVariance);

            var candidate = playerTransform.position
                + new Vector3(Mathf.Cos(angleRad), 0f, Mathf.Sin(angleRad)) * dist;
            if (Physics.Raycast(candidate, Vector3.down, out RaycastHit hit, dist))
            {
                // Не спавнити всередині frustum камери
                if (IsInsideCameraFrustum(hit.transform.position))
                    continue;

                return hit.transform.position;
            }
        }

        Debug.LogWarning("[SpawnManager] Не вдалось знайти позицію спавну за 30 спроб.");
        return null;
    }

    private bool IsInsideCameraFrustum(Vector3 worldPos)
    {
        if (_mainCamera == null) return false;

        var planes = GeometryUtility.CalculateFrustumPlanes(_mainCamera);
        var bounds = new Bounds(worldPos, Vector3.one * 0.5f);
        return GeometryUtility.TestPlanesAABB(planes, bounds);
    }

    // ── Пул об'єктів ──────────────────────────────────────────────────────────

    private GameObject GetFromPool(EnemyData data)
    {
        if (data == null || data.prefab == null)
        {
            Debug.LogError("[SpawnManager] EnemyData або prefab = null.");
            return null;
        }

        if (!_pools.ContainsKey(data))
            _pools[data] = new Queue<GameObject>();

        var pool = _pools[data];

        if (pool.Count > 0)
        {
            var pooled = pool.Dequeue();
            pooled.GetComponent<EnemyHealth>()?.ResetForPool();
            return pooled;
        }

        // Новий об'єкт — зберігаємо посилання на EnemyData
        var fresh = Instantiate(data.prefab);
        var health = fresh.GetComponent<EnemyHealth>();
        if (health != null) health.SourceData = data;
        return fresh;
    }

    /// <summary>
    /// Повертає ворога в пул після смерті. Викликається з EnemyHealth.
    /// </summary>
    public void ReturnToPool(GameObject go, EnemyData data)
    {
        go.SetActive(false);

        if (!_pools.ContainsKey(data))
            _pools[data] = new Queue<GameObject>();

        _pools[data].Enqueue(go);
    }

    /// <summary>
    /// Зменшує лічильник живих ворогів. Викликається з EnemyHealth при смерті.
    /// </summary>
    public void OnEnemyDied()
    {
        currentOnScreen = Mathf.Max(0, currentOnScreen - 1);
    }

    // ── Вибір ворога за вагою ─────────────────────────────────────────────────

    private EnemyData PickWeightedEnemy(EnemyData[] pool)
    {
        if (pool == null || pool.Length == 0) return null;

        int totalWeight = 0;
        foreach (var e in pool)
            if (e != null) totalWeight += e.spawnWeight;

        if (totalWeight <= 0) return pool[0];

        int roll = Random.Range(0, totalWeight);
        int acc  = 0;

        foreach (var e in pool)
        {
            if (e == null) continue;
            acc += e.spawnWeight;
            if (roll < acc) return e;
        }

        return pool[0];
    }

    // ── Debug / ContextMenu ───────────────────────────────────────────────────

    /// <summary>
    /// [Тест] Запускає орду першого HordeEvent одразу.
    /// </summary>
    [ContextMenu("Force Spawn Horde")]
    private void DebugForceSpawnHorde()
    {
        if (hordeEvents.Count == 0)
        {
            Debug.LogWarning("[SpawnManager] Немає налаштованих HordeEvents.");
            return;
        }

        var config = hordeEvents[0];
        StartCoroutine(SpawnHorde(config));
    }

    /// <summary>
    /// [Тест] Встановлює час старту третьої хвилі (індекс 2).
    /// </summary>
    [ContextMenu("Skip To Wave 3")]
    private void DebugSkipToWave3()
    {
        if (waves.Count < 3)
        {
            Debug.LogWarning("[SpawnManager] Менше 3 хвиль налаштовано.");
            return;
        }

        currentTime = waves[2].startTime;
        UpdateWave();
        Debug.Log($"[SpawnManager] Перемотано до хвилі 3 (t={currentTime}s).");
    }

    /// <summary>
    /// [Тест] Встановлює поточний час на 600 секунд (10 хвилин).
    /// </summary>
    [ContextMenu("Set Time 600s")]
    private void DebugSetTime600()
    {
        currentTime = 600f;
        UpdateWave();
        Debug.Log("[SpawnManager] Час встановлено: 600s.");
    }
}

// ── Структури даних ───────────────────────────────────────────────────────────

/// <summary>
/// Конфігурація однієї хвилі ворогів.
/// startTime у секундах (60 = 1 хвилина).
/// </summary>
[System.Serializable]
public class WaveConfig
{
    [Tooltip("Час старту хвилі в секундах")]
    public float startTime;

    [Tooltip("Пул ворогів для цієї хвилі")]
    public EnemyData[] enemyPool;

    [Tooltip("Інтервал між спавнами в секундах")]
    public float spawnInterval = 1.5f;

    [Tooltip("Максимальна кількість ворогів на екрані")]
    public int maxOnScreen = 30;

    [Tooltip("Множник HP ворогів (1 = базовий)")]
    public float hpMultiplier    = 1f;

    [Tooltip("Множник швидкості ворогів (1 = базовий)")]
    public float speedMultiplier = 1f;
}

/// <summary>
/// Конфігурація події орди — раптовий наплив ворогів з одного боку.
/// </summary>
[System.Serializable]
public class HordeConfig
{
    [Tooltip("Час активації орди в секундах")]
    public float triggerTime;

    [Tooltip("Тип ворогів орди")]
    public EnemyData enemyType;

    [Tooltip("Кількість ворогів в орді (рекомендовано 30–80)")]
    [Range(1, 100)]
    public int hordeCount = 40;

    [Tooltip("Встановлюється автоматично після активації")]
    public bool triggered;
}
