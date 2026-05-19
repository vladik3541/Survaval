using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private CardSelectionController cardController;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private GameObject player;

    [Header("Skill Database")]
    [SerializeField] private List<ActiveSkillData> allActiveSkills;
    [SerializeField] private List<PassiveSkillData> allPassiveSkills;

    [Header("Settings")]
    [SerializeField] private int cardsToShow = 3;

    // Поточний стан — яку карточку обрати
    private List<SkillData> _currentOfferedSkills = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Init(PlayerStats playerStats)
    {
        cardController.OnCardChosen += OnCardChosen;
        this.playerStats = playerStats;
        player = playerStats.gameObject;
    }

    // Головний метод — повертає N рандомних скілів для показу
    public List<SkillData> GetRandomCards(int count)
    {
        var pool = BuildWeightedPool();

        _currentOfferedSkills.Clear();
        var shuffled = WeightedShuffle(pool);

        foreach (var skill in shuffled)
        {
            if (_currentOfferedSkills.Count >= count) break;
            // Не дублювати в одному виборі
            if (!_currentOfferedSkills.Contains(skill))
                _currentOfferedSkills.Add(skill);
        }

        return _currentOfferedSkills;
    }

    // Викликається з CardSelectionController коли гравець вибрав карточку
    public void OnCardChosen(int index)
    {
        if (index < 0 || index >= _currentOfferedSkills.Count) return;

        var chosen = _currentOfferedSkills[index];

        if (chosen is ActiveSkillData activeData)
            ApplyActiveSkill(activeData);
        else if (chosen is PassiveSkillData passiveData)
            ApplyPassiveSkill(passiveData);
    }

    // ─── Активний скіл ───────────────────────────────────────────

    private void ApplyActiveSkill(ActiveSkillData data)
    {
        // Шукаємо чи вже є такий скіл на гравці
        var existing = GetActiveSkillComponent(data);

        if (existing != null)
        {
            existing.LevelUp();
        }
        else
        {
            // Створюємо дочірній об'єкт і додаємо компонент
            var skillObject = new GameObject(data.skillName);
            skillObject.transform.localPosition = Vector3.zero;
            skillObject.transform.SetParent(player.transform);

            // Тут потрібен mapping: дані → тип компонента
            // Варіант 1 — через словник типів
            var skillComponent = AddSkillComponent(skillObject, data);
            if (skillComponent != null)
                skillComponent.Initialize(data, playerStats);
        }
    }

    private BaseActiveSkill GetActiveSkillComponent(ActiveSkillData data)
    {
        // Перевірка: чи є вже скіл з таким ім'ям серед дітей
        foreach (Transform child in player.transform)
        {
            var skill = child.GetComponent<BaseActiveSkill>();
            if (skill != null && skill.Data == data)
                return skill;
        }
        return null;
    }

    private BaseActiveSkill AddSkillComponent(GameObject obj, ActiveSkillData data)
    {
        // Маппінг через Dictionary — реєструй свої скіли тут
        var map = new Dictionary<string, Func<BaseActiveSkill>>
        {
            { "Shuriken",  () => obj.AddComponent<ShurikenSkill>() },
            { "Shield",  () => obj.AddComponent<ShieldSkill>() }
            
        };

        if (map.TryGetValue(data.skillName, out var factory))
            return factory();

        Debug.LogWarning($"Невідомий скіл: {data.skillName}");
        return null;
    }

    // ─── Пасивний скіл ───────────────────────────────────────────

    private void ApplyPassiveSkill(PassiveSkillData data)
    {
        playerStats.ApplyPassive(data);
    }

    // ─── Рандом з вагами ─────────────────────────────────────────

    private List<SkillData> BuildWeightedPool()
    {
        var pool = new List<SkillData>();

        // Активні скіли
        foreach (var skill in allActiveSkills)
        {
            var existing = GetActiveSkillComponent(skill);
            int currentLevel = existing?.CurrentLevel ?? 0;

            // Пропускаємо якщо вже максимальний рівень
            if (currentLevel >= skill.maxLevel) continue;

            // Якщо вже є — підвищуємо вагу (апгрейди випадають частіше)
            int effectiveWeight = currentLevel > 0 ? skill.weight * 2 : skill.weight;
            for (int i = 0; i < effectiveWeight; i++)
                pool.Add(skill);
        }

        // Пасивні скіли — завжди доступні
        foreach (var skill in allPassiveSkills)
        {
            for (int i = 0; i < skill.weight; i++)
                pool.Add(skill);
        }

        return pool;
    }

    private List<SkillData> WeightedShuffle(List<SkillData> pool)
    {
        // Fisher-Yates shuffle по пулу з вагами
        var result = new List<SkillData>(pool);
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }
        // Прибираємо дублікати але зберігаємо порядок ваг
        return result.Distinct().ToList();
    }

    private void OnDestroy()
    {
        if (cardController != null)
            cardController.OnCardChosen -= OnCardChosen;
    }
}