using System;
using UnityEngine;

/// <summary>
/// Рахує кіли за поточний ран.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    /// <summary>Подія — передає нову кількість кілів.</summary>
    public event Action<int> OnKillCountChanged;

    [Header("Стан")]
    [SerializeField] private int killCount;

    public int KillCount => killCount;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>Додає 1 кіл і повідомляє підписників.</summary>
    public void AddKill()
    {
        killCount++;
        OnKillCountChanged?.Invoke(killCount);
    }
}
