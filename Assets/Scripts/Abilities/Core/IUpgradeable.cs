/// <summary>
/// Contract for any ability that supports level-based upgrades.
/// </summary>
public interface IUpgradeable
{
    /// <summary>Increment the ability level and apply the associated upgrade.</summary>
    void Upgrade();

    /// <summary>Current level (1-based, starts at 1).</summary>
    int CurrentLevel { get; }

    /// <summary>Hard cap on how many times this ability can be upgraded.</summary>
    int MaxLevel { get; }
}
