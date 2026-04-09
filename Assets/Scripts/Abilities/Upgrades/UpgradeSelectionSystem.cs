using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that manages the upgrade/skill selection flow triggered when the player levels up.
///
/// Two parallel selection modes exist:
///   1. Skill selection (preferred) — works with <see cref="SkillDefinition"/> + <see cref="PlayerSkillManager"/>.
///      Uses weighted randomisation: unowned skills have full weight, owned-but-not-maxed skills
///      have a reduced weight (<see cref="SkillDefinition.ownedWeightMultiplier"/>),
///      and fully maxed skills are excluded entirely.
///   2. Legacy stat-upgrade selection — works with <see cref="AbilityUpgradeData"/> (kept for compatibility).
///
/// Wire <see cref="OnSkillsReady"/> or <see cref="OnUpgradesReady"/> from your UI panel.
/// </summary>
public class UpgradeSelectionSystem : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────────
    public static UpgradeSelectionSystem Instance { get; private set; }

    // ── Legacy Data ───────────────────────────────────────────────────────────
    [Header("Legacy Upgrade Pool (stat upgrades)")]
    [Tooltip("AbilityUpgradeData assets for the old stat-upgrade flow.")]
    [SerializeField] private List<AbilityUpgradeData> allPossibleUpgrades;

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Fired when a skill selection has been prepared.
    /// Subscribe from your UI to receive the options and display them.
    /// </summary>
    public event Action<SkillDefinition[]> OnSkillsReady;

    /// <summary>Legacy event for the stat-upgrade flow.</summary>
    public event Action<AbilityUpgradeData[]> OnUpgradesReady;

    // ── Internal State ────────────────────────────────────────────────────────
    private BaseAbility _pendingAbility;

    // ── Unity Lifecycle ───────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // SKILL SELECTION (weighted, level-aware)
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Builds a weighted pool from all <see cref="SkillDefinition"/> entries registered in
    /// <see cref="PlayerSkillManager"/>, then returns up to <paramref name="count"/> unique skills.
    ///
    /// Weight rules:
    /// <list type="bullet">
    ///   <item>Maxed skill → excluded (weight = 0).</item>
    ///   <item>Owned but not maxed → weight = <see cref="SkillDefinition.ownedWeightMultiplier"/> (default 0.3).</item>
    ///   <item>Not yet owned → weight = 1.0 (full).</item>
    /// </list>
    /// Returns fewer than <paramref name="count"/> entries if the available pool is smaller.
    /// </summary>
    /// <param name="count">Maximum number of distinct skill options to return.</param>
    public SkillDefinition[] GetRandomSkills(int count = 3)
    {
        PlayerSkillManager manager = PlayerSkillManager.Instance;
        if (manager == null || manager.AllSkills == null || manager.AllSkills.Count == 0)
            return Array.Empty<SkillDefinition>();

        // Build the weighted pool (maxed skills are excluded).
        var weightedPool = new List<(SkillDefinition skill, float weight)>(manager.AllSkills.Count);
        foreach (SkillDefinition skill in manager.AllSkills)
        {
            if (manager.IsSkillMaxed(skill)) continue;

            int   level  = manager.GetSkillLevel(skill);
            float weight = level > 0 ? skill.ownedWeightMultiplier : 1f;
            weightedPool.Add((skill, weight));
        }

        if (weightedPool.Count == 0)
            return Array.Empty<SkillDefinition>();

        // Weighted selection without replacement.
        int                    actual = Mathf.Min(count, weightedPool.Count);
        List<SkillDefinition>  result = new List<SkillDefinition>(actual);

        for (int i = 0; i < actual; i++)
        {
            float totalWeight = 0f;
            foreach (var entry in weightedPool)
                totalWeight += entry.weight;

            float roll        = UnityEngine.Random.Range(0f, totalWeight);
            float accumulated = 0f;

            for (int j = 0; j < weightedPool.Count; j++)
            {
                accumulated += weightedPool[j].weight;
                if (roll <= accumulated)
                {
                    result.Add(weightedPool[j].skill);
                    weightedPool.RemoveAt(j);
                    break;
                }
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// Pauses the game, selects up to 3 skills via <see cref="GetRandomSkills"/>,
    /// and broadcasts them through <see cref="OnSkillsReady"/>.
    /// </summary>
    public void TriggerSkillSelection()
    {
        Time.timeScale = 0f;

        SkillDefinition[] choices = GetRandomSkills(3);
        if (choices.Length == 0)
        {
            Debug.LogWarning("[UpgradeSelectionSystem] No skills available for selection — resuming game.");
            ResumeGame();
            return;
        }

        OnSkillsReady?.Invoke(choices);
    }

    /// <summary>
    /// Applies the player's chosen skill via <see cref="PlayerSkillManager.AddOrUpgrade"/>
    /// and resumes the game.
    /// </summary>
    /// <param name="skill">The skill the player selected.</param>
    public void ApplySkill(SkillDefinition skill)
    {
        if (skill == null) return;

        PlayerSkillManager.Instance?.AddOrUpgrade(skill);
        ResumeGame();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // LEGACY STAT-UPGRADE SELECTION (AbilityUpgradeData)
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns <paramref name="count"/> randomly selected, unique <see cref="AbilityUpgradeData"/>
    /// assets from the legacy global pool. Selection is unweighted.
    /// Returns fewer than <paramref name="count"/> if the pool is too small.
    /// </summary>
    public AbilityUpgradeData[] GetRandomUpgrades(int count = 3)
    {
        if (allPossibleUpgrades == null || allPossibleUpgrades.Count == 0)
            return Array.Empty<AbilityUpgradeData>();

        List<AbilityUpgradeData> pool   = new List<AbilityUpgradeData>(allPossibleUpgrades);
        List<AbilityUpgradeData> result = new List<AbilityUpgradeData>(count);
        int                      actual = Mathf.Min(count, pool.Count);

        for (int i = 0; i < actual; i++)
        {
            int index = UnityEngine.Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result.ToArray();
    }

    /// <summary>
    /// Applies a selected legacy stat-upgrade to an ability and resumes the game.
    /// </summary>
    /// <param name="upgrade">The upgrade the player chose.</param>
    /// <param name="ability">The ability to upgrade.</param>
    public void ApplyUpgrade(AbilityUpgradeData upgrade, BaseAbility ability)
    {
        if (upgrade == null || ability == null) return;

        ability.Upgrade();
        ResumeGame();
    }

    /// <summary>
    /// Pauses the game and broadcasts 3 random legacy stat-upgrades via <see cref="OnUpgradesReady"/>.
    /// </summary>
    /// <param name="sourceAbility">Optional: the ability that caused this level-up trigger.</param>
    public void TriggerUpgradeSelection(BaseAbility sourceAbility = null)
    {
        _pendingAbility = sourceAbility;
        Time.timeScale  = 0f;

        AbilityUpgradeData[] choices = GetRandomUpgrades(3);
        OnUpgradesReady?.Invoke(choices);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // SHARED
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>Resumes normal game flow. Called automatically after any Apply* method.</summary>
    public void ResumeGame()
    {
        Time.timeScale  = 1f;
        _pendingAbility = null;
    }
}
