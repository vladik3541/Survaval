using UnityEngine;

/// <summary>
/// ScriptableObject — дані одного героя.
/// Створюється через: Assets → Create → Heroes → HeroData
/// </summary>
[CreateAssetMenu(fileName = "NewHero", menuName = "Heroes/HeroData")]
public class HeroData : ScriptableObject
{
    [Header("Ідентифікатор (унікальний рядок)")]
    public string heroId = "hero_default";

    [Header("Відображення")]
    public string displayName = "Герой";
    [TextArea] public string description = "Опис героя.";
    public Sprite icon;
    public Sprite fullArtwork;

    [Header("Доступність")]
    public bool isDefaultUnlocked = false;  // true = безкоштовний, відкритий з початку
    public int price = 500;                  // ціна в монетах якщо заблокований

    [Header("Базові характеристики")]
    public float baseHealth           = 100f;
    public float baseSpeed            = 5f;
    public float baseDamageMultiplier = 1f;
    public float basePickupRadius     = 3f;

    [Header("Префаб персонажа (для ігрової сцени)")]
    public GameObject characterPrefab;
}
