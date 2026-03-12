using UnityEngine;

/// <summary>
/// Компонент на prefab-і кристала. Зберігає GemData.
/// GemCollector читає це при підборі.
/// </summary>
public class Gem : MonoBehaviour
{
    [Header("Дані кристала")]
    [SerializeField] private GemData data;

    public GemData Data => data;
}
