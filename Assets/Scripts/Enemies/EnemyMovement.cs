using UnityEngine;

/// <summary>
/// Рух ворога до гравця через Rigidbody.
/// Наносить шкоду гравцю поки знаходиться в зоні атаки (TriggerStay).
///
/// Налаштування GameObject:
/// - Rigidbody: Freeze Rotation XYZ = true
/// - Collider (тіло) — Is Trigger = false
/// - Дочірній GameObject "AttackZone" — Collider з Is Trigger = true
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Рух")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("Відстань, ближче якої ворог зупиняється і не штовхає гравця")]
    [SerializeField] private float stopDistance = 1.2f;

    [Header("Атака")]
    [SerializeField] private float damage         = 10f;
    [SerializeField] private float damageInterval = 1f;

    private Rigidbody    _rb;
    private Transform    _player;
    private float        _damageTimer;

    // ── Ініціалізація ─────────────────────────────────────────────────────────

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }

    private void Start()
    {
        // Шукаємо гравця по тегу
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;
        else
            Debug.LogWarning($"[EnemyMovement] Гравець з тегом 'Player' не знайдений.");
    }

    // ── Рух ──────────────────────────────────────────────────────────────────

    private void FixedUpdate()
    {
        if (_player == null) return;

        Vector3 toPlayer = _player.position - transform.position;
        toPlayer.y = 0f; // рух лише по горизонталі

        float distance = toPlayer.magnitude;

        if (distance > stopDistance)
        {
            Vector3 dir = toPlayer.normalized;
            _rb.MovePosition(_rb.position + dir * (moveSpeed * Time.fixedDeltaTime));

            // Поворот в бік гравця
            _rb.rotation = Quaternion.LookRotation(dir);
        }
    }

    // ── Атака через Trigger ───────────────────────────────────────────────────

    /// <summary>
    /// Викликається кожен фізичний крок поки гравець у зоні атаки.
    /// Шкода наноситься не частіше ніж раз на damageInterval секунд.
    /// </summary>
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _damageTimer -= Time.fixedDeltaTime;
        if (_damageTimer > 0f) return;

        _damageTimer = damageInterval;

        var health = other.GetComponent<PlayerHealth>();
        health?.TakeDamage(damage);
    }

    private void OnTriggerEnter(Collider other)
    {
        // При вході — одразу наносимо перший удар (скидаємо таймер)
        if (other.CompareTag("Player"))
            _damageTimer = 0f;
    }
}
