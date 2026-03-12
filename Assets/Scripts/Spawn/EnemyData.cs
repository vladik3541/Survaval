using UnityEngine;

/// <summary>
/// Дані про тип ворога: префаб та вага спавну для системи хвиль.
/// </summary>
[CreateAssetMenu(menuName = "Enemies/EnemyData", fileName = "NewEnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Загальне")]
    public string      enemyName   = "Enemy";
    public GameObject  prefab;

    [Header("Вага спавну")]
    [Tooltip("Чим вище — тим частіше спавниться (1–10)")]
    [Range(1, 10)]
    public int spawnWeight = 1;
}
