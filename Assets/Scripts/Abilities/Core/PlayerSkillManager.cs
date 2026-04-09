using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the player's current skill collection.
/// Tracks which skills have been acquired (and at what level), and handles
/// adding a new skill or upgrading an existing one when the player makes a choice.
///
/// Place this component on the player root GameObject.
/// Wire <see cref="allSkillDefinitions"/> in the Inspector with every
/// SkillDefinition asset that should be available during the run.
/// </summary>
public class PlayerSkillManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static PlayerSkillManager Instance { get; private set; }

    // ── Data ──────────────────────────────────────────────────────────────────
    [Header("Skill Pool")]
    [Tooltip("Every SkillDefinition that can appear during this run.")]
    [SerializeField] private SkillDefinition[] allSkillDefinitions;

    // ── Runtime State ─────────────────────────────────────────────────────────
    // Maps skill id → the live BaseAbility component on the player.
    private readonly Dictionary<string, BaseAbility> _ownedAbilities = new();

    // ── Public Read-Only Access ───────────────────────────────────────────────
    /// <summary>All skill definitions registered for this run.</summary>
    public IReadOnlyList<SkillDefinition> AllSkills => allSkillDefinitions;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ── Public Queries ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the current level of <paramref name="skill"/> on this player.
    /// Returns 0 if the skill has not been acquired yet.
    /// </summary>
    public int GetSkillLevel(SkillDefinition skill)
    {
        if (skill == null) return 0;
        return _ownedAbilities.TryGetValue(skill.id, out BaseAbility ab) ? ab.CurrentLevel : 0;
    }

    /// <summary>
    /// Returns true when the skill is owned and has reached its maximum level,
    /// meaning it must be excluded from the selection pool entirely.
    /// </summary>
    public bool IsSkillMaxed(SkillDefinition skill)
    {
        if (skill == null) return true;
        return GetSkillLevel(skill) >= skill.maxLevel;
    }

    // ── Public Mutations ──────────────────────────────────────────────────────

    /// <summary>
    /// Adds a new skill to the player (level 1) or upgrades an existing one by 1 level.
    /// All effects are applied immediately via <see cref="BaseAbility.Upgrade"/>.
    /// </summary>
    /// <param name="skill">The skill chosen by the player.</param>
    public void AddOrUpgrade(SkillDefinition skill)
    {
        if (skill == null)
        {
            Debug.LogError("[PlayerSkillManager] AddOrUpgrade: skill is null.", this);
            return;
        }

        if (_ownedAbilities.TryGetValue(skill.id, out BaseAbility existing))
        {
            // Skill already owned — level it up.
            existing.Upgrade();
            Debug.Log($"[PlayerSkillManager] '{skill.skillName}' upgraded to level {existing.CurrentLevel}.", this);
        }
        else
        {
            // New skill — instantiate the prefab as a child and register it.
            if (skill.abilityPrefab == null)
            {
                Debug.LogError($"[PlayerSkillManager] SkillDefinition '{skill.id}' has no abilityPrefab assigned.", this);
                return;
            }

            GameObject abilityGO = Instantiate(skill.abilityPrefab, transform);
            BaseAbility ability   = abilityGO.GetComponent<BaseAbility>();

            if (ability == null)
            {
                Debug.LogError($"[PlayerSkillManager] Prefab for skill '{skill.id}' has no BaseAbility component.", this);
                Destroy(abilityGO);
                return;
            }

            _ownedAbilities[skill.id] = ability;
            Debug.Log($"[PlayerSkillManager] '{skill.skillName}' acquired at level 1.", this);
        }
    }
}
