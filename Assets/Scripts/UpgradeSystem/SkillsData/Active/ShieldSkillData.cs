using UnityEngine;

[CreateAssetMenu(fileName = "ShieldSkillData", menuName = "Skills/Active/Shield")]
public class ShieldSkillData : ActiveSkillData
{
    [Header("Shield Prefab")]
    public GameObject shieldPrefab;

    [Header("Per Level Config")]
    public ShieldLevelConfig[] levelConfigs;
}

[System.Serializable]
public class ShieldLevelConfig
{
    public float shieldHp;
    public float cooldown;
}