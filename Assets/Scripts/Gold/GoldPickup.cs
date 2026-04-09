using UnityEngine;

/// <summary>
/// Компонент на prefab-і золота. Збирається через GemCollector на гравці.
/// </summary>
public class GoldPickup : MonoBehaviour
{
    [Header("Цінність")]
    [SerializeField] private int goldValue = 5;

    public int GoldValue => goldValue;
}
