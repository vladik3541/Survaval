using UnityEngine;

/// <summary>
/// Компонент на prefab-і аптечки. Збирається через GemCollector на гравці.
/// </summary>
public class HealthPickup : MonoBehaviour
{
    [Header("Відновлення HP")]
    [SerializeField] private float healAmount = 20f;

    public float HealAmount => healAmount;
}
