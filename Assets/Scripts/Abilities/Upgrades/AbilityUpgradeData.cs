using UnityEngine;

/// <summary>Type of stat modifier an upgrade applies.</summary>
public enum AbilityUpgradeType
{
    Damage,
    Cooldown,
    Pierce,
    MultiShot,
    AOE,
    Chain
}

/// <summary>
/// ScriptableObject that describes a single ability upgrade option shown in the selection UI.
/// Create via: Assets → Create → Ability → AbilityUpgradeData
/// </summary>
[CreateAssetMenu(fileName = "NewUpgradeData", menuName = "Ability/AbilityUpgradeData")]
public class AbilityUpgradeData : ScriptableObject
{
    [Header("Display")]
    public string upgradeName;
    [TextArea(2, 4)] public string upgradeDescription;
    public Sprite icon;

    [Header("Upgrade Definition")]
    public AbilityUpgradeType type;
    public float value;
}
