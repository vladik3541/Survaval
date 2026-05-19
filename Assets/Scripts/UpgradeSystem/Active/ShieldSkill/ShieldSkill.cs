using System.Collections;
using UnityEngine;

public class ShieldSkill : BaseActiveSkill
{
    private ShieldSkillData _data;

    private GameObject _shieldVisual;

    private float _currentHp;
    private float _maxHp;
    private bool _isAlive;
    private Coroutine _cooldownCoroutine;

    protected override void OnSkillAdded()
    {
        _data = Data as ShieldSkillData;
        SpawnShieldObject();
        ApplyLevelConfig();
    }

    protected override void OnLevelUp(int newLevel)
    {
        ApplyLevelConfig();
    }

    private void SpawnShieldObject()
    {
        _shieldVisual = Instantiate(
            _data.shieldPrefab,
            transform.position,
            Quaternion.identity,
            transform
        );
        _shieldVisual.transform.localPosition = Vector3.zero + Vector3.up;
    }

    private void ApplyLevelConfig()
    {
        int idx = Mathf.Clamp(CurrentLevel - 1, 0, _data.levelConfigs.Length - 1);
        ShieldLevelConfig cfg = _data.levelConfigs[idx];

        _maxHp = cfg.shieldHp;

        if (_isAlive)
        {
            _currentHp = _maxHp;
        }
    }

    public float AbsorbDamage(float incoming)
    {
        if (!_isAlive) return incoming;

        float absorbed = Mathf.Min(_currentHp, incoming);
        _currentHp -= absorbed;

        if (_currentHp <= 0f)
            BreakShield();

        return incoming - absorbed;
    }

    private void BreakShield()
    {
        _isAlive = false;

        _shieldVisual.SetActive(false);

        int idx = Mathf.Clamp(CurrentLevel - 1, 0, _data.levelConfigs.Length - 1);
        float cd = _data.levelConfigs[idx].cooldown;

        if (_cooldownCoroutine != null)
            StopCoroutine(_cooldownCoroutine);

        _cooldownCoroutine = StartCoroutine(RestoreRoutine(cd));
    }

    private IEnumerator RestoreRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);

        int idx = Mathf.Clamp(CurrentLevel - 1, 0, _data.levelConfigs.Length - 1);
        _maxHp = _data.levelConfigs[idx].shieldHp;
        _currentHp = _maxHp;
        _isAlive = true;

        _shieldVisual.SetActive(true);

        _cooldownCoroutine = null;
    }

    private void OnDestroy()
    {
        if (_cooldownCoroutine != null)
            StopCoroutine(_cooldownCoroutine);

        if (_shieldVisual != null)
            Destroy(_shieldVisual);
    }
}