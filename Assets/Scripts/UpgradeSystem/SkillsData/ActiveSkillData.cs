using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "ActiveSkill", menuName = "Skills/Active")]
public class ActiveSkillData : SkillData
{
    public int maxLevel = 5;
    public string[] levelDescriptions; // опис для кожного рівня
    public MonoScript skillScript; // який компонент додавати
}
