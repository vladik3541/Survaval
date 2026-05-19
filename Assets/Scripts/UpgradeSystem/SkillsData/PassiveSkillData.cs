using UnityEngine;

public enum PassiveSkillName
{
    speed,
    health,
    armor
}

[CreateAssetMenu(fileName = "PassiveSkill", menuName = "Skills/Passive")]
public class PassiveSkillData : SkillData
{
    public PassiveSkillName skillName;
    public float value;
}
