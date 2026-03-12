using UnityEngine;

/// <summary>
/// ScriptableObject з даними кристала досвіду.
/// </summary>
[CreateAssetMenu(menuName = "Loot/GemData", fileName = "GemData")]
public class GemData : ScriptableObject
{
    [Header("Досвід")]
    public float xpValue = 10f;
}
