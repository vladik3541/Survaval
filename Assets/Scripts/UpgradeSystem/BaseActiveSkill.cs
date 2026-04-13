using UnityEngine;

public abstract class BaseActiveSkill : MonoBehaviour
{
    public ActiveSkillData Data { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    protected PlayerStats Stats;

    public void Initialize(ActiveSkillData data, PlayerStats stats)
    {
        Data = data;
        Stats = stats;
        OnSkillAdded();
    }

    public void LevelUp()
    {
        if (CurrentLevel >= Data.maxLevel) return;
        CurrentLevel++;
        OnLevelUp(CurrentLevel);
    }

    protected virtual void OnSkillAdded() { }
    protected virtual void OnLevelUp(int newLevel) { }
}