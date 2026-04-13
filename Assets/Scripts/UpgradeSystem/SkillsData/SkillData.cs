using UnityEngine;

public abstract class SkillData : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    [TextArea] public string description;
    public int weight = 10; // ймовірність випадання
}