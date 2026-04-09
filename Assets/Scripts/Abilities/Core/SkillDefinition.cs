using UnityEngine;

/// <summary>
/// Describes a single skill entry in the global skill pool.
/// Holds display data and the prefab to instantiate when the player acquires this skill.
/// Create via: Assets → Create → Ability → SkillDefinition
/// </summary>
[CreateAssetMenu(fileName = "NewSkillDefinition", menuName = "Ability/SkillDefinition")]
public class SkillDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Must be unique across all SkillDefinitions in the project.")]
    public string id;
    public string skillName;
    [TextArea(2, 4)] public string description;
    public Sprite icon;

    [Header("Pool Settings")]
    [Tooltip("Hard cap on how many times this skill can be upgraded. " +
             "Should match the BaseAbility.MaxLevel of the linked prefab.")]
    public int maxLevel = 5;

    [Range(0f, 1f)]
    [Tooltip("Weight multiplier applied when the player already owns this skill but hasn't maxed it. " +
             "1 = full weight (same as unowned), 0 = never appears as an upgrade.")]
    public float ownedWeightMultiplier = 0.3f;

    [Header("Ability")]
    [Tooltip("Prefab that carries the concrete BaseAbility MonoBehaviour. " +
             "Instantiated as a child of the player on first acquisition.")]
    public GameObject abilityPrefab;
}
