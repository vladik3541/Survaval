using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// HP ворога: смерть → дроп лута, очко до ScoreManager, dissolve-ефект,
/// повернення в пул SpawnManager (або Destroy якщо пул недоступний).
/// </summary>
public class EnemyHealth : Health
{
    [Header("Dissolve (матеріал з параметром _DissolveAmount)")]
    [SerializeField] private Renderer dissolveRenderer;
    [SerializeField] private float    dissolveDuration = 0.8f;

    private const string DissolveProperty = "_DissolveAmount";

    // Базові значення зберігаються один раз при Awake (для скидання з пулу)
    private float _baseMaxHealth;
    private float _baseAgentSpeed;

    // Клонований матеріал (один на весь час життя об'єкта)
    private Material _instanceMaterial;

    private bool _isDead;

    /// <summary>EnemyData, з якого було заспавнено. Встановлюється SpawnManager.</summary>
    public EnemyData SourceData { get; set; }

    // ── Ініціалізація ─────────────────────────────────────────────────────────

    protected override void Awake()
    {
        base.Awake();

        _baseMaxHealth = maxHealth;

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) _baseAgentSpeed = agent.speed;

        // Клонуємо матеріал один раз — щоб не змінювати shared asset
        if (dissolveRenderer != null)
            _instanceMaterial = dissolveRenderer.material;
    }

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        var hitEffect = ServiceLocator.Get<PoolService>().Get(PoolName.hitEnemy, transform.position+Vector3.up, Quaternion.identity);
        hitEffect.GetComponent<Poolable>().ReturnToPool(1);
    }
    
    // ── Публічний API ─────────────────────────────────────────────────────────

    /// <summary>
    /// Застосовує множники HP та швидкості відповідно до поточної хвилі.
    /// Викликається SpawnManager після отримання об'єкта з пулу.
    /// </summary>
    public void ApplyMultipliers(float hpMultiplier, float speedMultiplier)
    {
        maxHealth      = _baseMaxHealth * hpMultiplier;
        currentHealth  = maxHealth;

        var agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.speed = _baseAgentSpeed * speedMultiplier;
    }

    /// <summary>
    /// Скидає стан ворога для повторного використання з пулу.
    /// Викликається SpawnManager перед SetActive(true).
    /// </summary>
    public void ResetForPool()
    {
        _isDead       = false;
        maxHealth     = _baseMaxHealth;
        currentHealth = maxHealth;

        // Скидаємо dissolve
        if (_instanceMaterial != null)
        {
            _instanceMaterial.DOKill();
            _instanceMaterial.SetFloat(DissolveProperty, 0f);
        }

        // Вмикаємо колайдер
        var col = GetComponent<Collider>();
        if (col != null) col.enabled = true;

        // Скидаємо швидкість
        var agent = GetComponent<NavMeshAgent>();
        if (agent != null) agent.speed = _baseAgentSpeed;
    }

    // ── Смерть ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Дропає лут, нараховує кіл, програє dissolve-ефект
    /// і повертає об'єкт в пул (або видаляє, якщо пул відсутній).
    /// </summary>
    public override void Die()
    {
        if (_isDead) return;
        _isDead = true;

        OnDeath?.Invoke();

        var dieEffect = ServiceLocator.Get<PoolService>()
            .Get(PoolName.dieEnemy, transform.position + Vector3.up, Quaternion.identity).GetComponent<ParticleSystem>();
        dieEffect.Emit(1);
        dieEffect.GetComponent<Poolable>().ReturnToPool(0.2f); 
        // Повідомляємо SpawnManager про смерть — зменшує лічильник
        SpawnManager.Instance?.OnEnemyDied();

        // 1. Дроп лута
        LootDropper.Drop(transform.position + Vector3.up);

        // 2. Рахунок килів
        ScoreManager.Instance?.AddKill();

        // 3. Dissolve + повернення в пул
        PlayDissolveAndReturn();
        AudioManager.Instance.PlayEnemyKill();
    }

    // ── Приватне ──────────────────────────────────────────────────────────────

    private void PlayDissolveAndReturn()
    {
        // Вимикаємо колайдер, щоб мертвий ворог не блокував
        var col = GetComponent<Collider>();
        var rb = GetComponent<Rigidbody>();
        if (col != null) col.enabled = false;
        if (rb != null) rb.isKinematic = true;

        
        ReturnOrDestroy();
    }

    private void ReturnOrDestroy()
    {
        if (SpawnManager.Instance != null && SourceData != null)
            SpawnManager.Instance.ReturnToPool(gameObject, SourceData);
        else
            Destroy(gameObject);
    }
}
