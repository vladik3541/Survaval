using UnityEngine;

// Компонент на самому prefab сюрікена — відповідає за орбіту
public class ShurikenOrbitController : MonoBehaviour
{
    [HideInInspector] public float orbitSpeed;
    [HideInInspector] public float angleOffset;

    private float _currentAngle;
    private Transform _playerTransform;
    private float _damage;

    private void Awake()
    {
        // Піднімаємось по ієрархії: ShurikenPrefab → ShurikenSkill obj → Player
        _playerTransform = GetComponentInParent<PlayerStats>().transform;

        if (_playerTransform == null)
            Debug.LogWarning("ShurikenOrbitController: не знайдено transform гравця!");
    }
    public void Init(float damage)
    {
        _damage = damage;
    }
    private void Update()
    {
        if (_playerTransform == null) return;

        _currentAngle += orbitSpeed * Time.deltaTime;
        float totalAngle = _currentAngle + angleOffset;

        float radius = GetComponentInParent<ShurikenSkill>() is ShurikenSkill skill
            ? (skill.Data as ShurikenSkillData)?.orbitRadius ?? 2f
            : 2f;

        float rad = totalAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * radius;

        transform.position = _playerTransform.position + offset;

        // Обертаємо сам сюрікен навколо своєї осі для краси
        transform.Rotate(Vector3.up, orbitSpeed * 2f * Time.deltaTime, Space.World);
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out EnemyHealth enemyHealth))
        {
            enemyHealth.TakeDamage(_damage);
        }
    }
}