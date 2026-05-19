using System.Collections.Generic;
using UnityEngine;

public class ShurikenSkill : BaseActiveSkill
{
    private ShurikenSkillData _shurikenData;
    private readonly List<GameObject> _shurikens = new();

    protected override void OnSkillAdded()
    {
        _shurikenData = Data as ShurikenSkillData;
        ApplyLevelConfig();
    }

    protected override void OnLevelUp(int newLevel)
    {
        ApplyLevelConfig();
    }

    private void ApplyLevelConfig()
    {
        if (_shurikenData == null) return;

        // Індекс конфіга = рівень - 1
        int idx = Mathf.Clamp(CurrentLevel - 1, 0, _shurikenData.levelConfigs.Length - 1);
        ShurikenLevelConfig config = _shurikenData.levelConfigs[idx];

        AdjustShurikenCount(config.shurikenCount);
        UpdateOrbitSpeed(config.orbitSpeed);
    }

    // Додає або видаляє сюрікени до потрібної кількості
    private void AdjustShurikenCount(int targetCount)
    {
        // Додаємо яких не вистачає
        while (_shurikens.Count < targetCount)
        {
            GameObject shuriken = Instantiate(
                _shurikenData.shurikenPrefab,
                transform.position,
                Quaternion.identity,
                transform   // дочірній до об'єкта скіла
            );
            _shurikens.Add(shuriken);
        }

        // Видаляємо зайві (на випадок downgrade або ребалансу)
        while (_shurikens.Count > targetCount)
        {
            int last = _shurikens.Count - 1;
            Destroy(_shurikens[last]);
            _shurikens.RemoveAt(last);
        }
    }

    // Оновлює швидкість на OrbitController кожного сюрікена
    private void UpdateOrbitSpeed(float speed)
    {
        foreach (var shuriken in _shurikens)
        {
            if (shuriken == null) continue;
            var orbit = shuriken.GetComponent<ShurikenOrbitController>();
            orbit.Init(_shurikenData.levelConfigs[0].damage);
            if (orbit != null)
                orbit.orbitSpeed = speed;
        }
    }

    private void Update()
    {
        if (_shurikens.Count == 0) return;

        // Рівномірно розподіляємо сюрікени по колу
        float angleStep = 360f / _shurikens.Count;

        for (int i = 0; i < _shurikens.Count; i++)
        {
            if (_shurikens[i] == null) continue;

            var orbit = _shurikens[i].GetComponent<ShurikenOrbitController>();
            if (orbit != null)
                orbit.angleOffset = angleStep * i;
        }
    }

    private void OnDestroy()
    {
        foreach (var shuriken in _shurikens)
            if (shuriken != null) Destroy(shuriken);

        _shurikens.Clear();
    }
}