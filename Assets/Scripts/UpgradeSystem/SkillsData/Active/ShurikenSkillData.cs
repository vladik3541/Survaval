using UnityEngine;

[CreateAssetMenu(fileName = "ShurikenSkillData", menuName = "Skills/Active/Shuriken")]
public class ShurikenSkillData : ActiveSkillData
{
    [Header("Shuriken Settings")]
    public GameObject shurikenPrefab;
    public float orbitRadius = 2f;

    [Header("Per Level Config")]
    public ShurikenLevelConfig[] levelConfigs;
}

[System.Serializable]
public class ShurikenLevelConfig
{
    public int shurikenCount;
    public float orbitSpeed;
}