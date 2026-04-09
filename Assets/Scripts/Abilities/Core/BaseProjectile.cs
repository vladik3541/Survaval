using UnityEngine;

/// <summary>
/// Abstract base for all projectiles.
/// Повернення в пул відбувається через компонент Poolable (ServiceLocator → PoolService).
/// Кожен prefab снаряда повинен мати компонент Poolable з PoolName, що збігається з AbilityData.poolName.
/// </summary>
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Poolable))]
public abstract class BaseProjectile : MonoBehaviour
{
    // ── Stats (set via Init) ────────────────────────────────────────────────
    protected float   damage;
    protected float   speed;
    protected float   range;

    // ── State ───────────────────────────────────────────────────────────────
    protected Vector3 startPosition;
    protected bool    hasHit;

    // ── Cached Components ───────────────────────────────────────────────────
    private Poolable poolable;

    // ── Layer ───────────────────────────────────────────────────────────────
    [Header("Detection")]
    [SerializeField] protected LayerMask enemyLayer;

    // ── Unity Lifecycle ─────────────────────────────────────────────────────
    protected virtual void Awake()
    {
        poolable = GetComponent<Poolable>();
        if (poolable == null)
            Debug.LogError($"[{GetType().Name}] Компонент Poolable відсутній на prefab-і {gameObject.name}!", this);
    }

    protected virtual void Update()
    {
        if (hasHit) return;

        Move();
        CheckRangeLimit();
    }

    // ── Public API ──────────────────────────────────────────────────────────

    /// <summary>
    /// Ініціалізує снаряд після отримання з пулу.
    /// Викликати одразу після PoolService.Get().
    /// </summary>
    public virtual void Init(float dmg, float spd, float rng, Vector3 direction)
    {
        damage        = dmg;
        speed         = spd;
        range         = rng;
        startPosition = transform.position;
        hasHit        = false;

        if (speed <= 0f)  Debug.LogWarning($"[{GetType().Name}] Init: speed={speed} (≤0, снаряд не рухатиметься)", this);
        if (range <= 0f)  Debug.LogWarning($"[{GetType().Name}] Init: range={range} (≤0, одразу повернеться в пул)", this);
        if (enemyLayer.value == 0) Debug.LogWarning($"[{GetType().Name}] enemyLayer не встановлено — колізії з ворогами не спрацюють", this);
    }

    /// <summary>Рух снаряда. Викликається кожен Update. За замовчуванням порожній.</summary>
    public virtual void Move() { }

    /// <summary>Логіка попадання. Реалізується в кожному класі снаряда.</summary>
    public abstract void OnHitEnemy(EnemyHealth enemy);

    // ── Collision ────────────────────────────────────────────────────────────
    protected virtual void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        bool isEnemyLayer = (enemyLayer.value & (1 << other.gameObject.layer)) != 0;
        Debug.Log($"[{GetType().Name}] OnTriggerEnter: {other.name} (layer={other.gameObject.layer}, isEnemy={isEnemyLayer})", this);

        if (!isEnemyLayer) return;

        EnemyHealth enemy = other.GetComponent<EnemyHealth>();
        if (enemy == null)
        {
            Debug.LogWarning($"[{GetType().Name}] Об'єкт {other.name} на Enemy layer але без EnemyHealth!", this);
            return;
        }

        OnHitEnemy(enemy);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────
    private void CheckRangeLimit()
    {
        if (Vector3.Distance(startPosition, transform.position) >= range)
            ReturnToPool();
    }

    /// <summary>
    /// Повертає снаряд у PoolService через компонент Poolable.
    /// </summary>
    protected void ReturnToPool()
    {
        hasHit = true;
        if (poolable != null)
            poolable.ReturnToPool();
        else
            gameObject.SetActive(false);
    }
}
