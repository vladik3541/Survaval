using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Сервіс збереження гри.
/// Зберігає дані у JSON-файл в Application.persistentDataPath/save.json.
/// Реєструється через ServiceLocator у GameBootstrap.
///
/// Доступ: ServiceLocator.Get&lt;SaveService&gt;()
/// </summary>
public class SaveService
{
    private SaveData _data;
    private readonly string _savePath;

    public bool IsFirstRun => !_data.initialized;

    /// <summary>Стріляє при кожній зміні кількості монет. Передає нове значення.</summary>
    public event Action<int> OnCoinsChanged;

    public SaveService()
    {
        _savePath = Path.Combine(Application.persistentDataPath, "save.json");
        Load();
    }

    // ── монети ───────────────────────────────────────────────────────────────

    public int Coins => _data.coins;

    /// <summary>Ініціалізує стартові монети при першому запуску.</summary>
    public void InitializeFirstRun(int startingCoins)
    {
        if (_data.initialized) return;
        _data.coins = startingCoins;
        _data.initialized = true;
        Save();
        OnCoinsChanged?.Invoke(_data.coins);
    }

    public void AddCoins(int amount)
    {
        _data.coins += amount;
        Save();
        OnCoinsChanged?.Invoke(_data.coins);
    }

    /// <summary>Витратити монети. Повертає false якщо монет не вистачає.</summary>
    public bool SpendCoins(int amount)
    {
        if (_data.coins < amount) return false;
        _data.coins -= amount;
        Save();
        OnCoinsChanged?.Invoke(_data.coins);
        return true;
    }

    public void SetCoins(int amount)
    {
        _data.coins = amount;
        Save();
        OnCoinsChanged?.Invoke(_data.coins);
    }

    // ── герої ────────────────────────────────────────────────────────────────

    public bool IsHeroUnlocked(string heroId)
    {
        return _data.purchasedHeroIds.Contains(heroId);
    }

    public void UnlockHero(string heroId)
    {
        if (string.IsNullOrEmpty(heroId)) return;
        if (!_data.purchasedHeroIds.Contains(heroId))
        {
            _data.purchasedHeroIds.Add(heroId);
            Save();
        }
    }

    // ── прокачка ─────────────────────────────────────────────────────────────

    public int GetPowerUpLevel(PowerUpType type)
    {
        var typeName = type.ToString();
        foreach (var entry in _data.powerUpLevels)
            if (entry.typeName == typeName) return entry.level;
        return 0;
    }

    public void SetPowerUpLevel(PowerUpType type, int level)
    {
        var typeName = type.ToString();
        foreach (var entry in _data.powerUpLevels)
        {
            if (entry.typeName == typeName)
            {
                entry.level = level;
                Save();
                return;
            }
        }
        _data.powerUpLevels.Add(new PowerUpSaveEntry { typeName = typeName, level = level });
        Save();
    }

    // ── файлові операції ──────────────────────────────────────────────────────

    public void Save()
    {
        try
        {
            var json = JsonUtility.ToJson(_data, true);
            File.WriteAllText(_savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] Помилка збереження: {e.Message}");
        }
    }

    public void Load()
    {
        try
        {
            if (File.Exists(_savePath))
            {
                var json = File.ReadAllText(_savePath);
                _data = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
                Debug.Log($"[SaveService] Збереження завантажено. Монети: {_data.coins}");
            }
            else
            {
                _data = new SaveData();
                Debug.Log("[SaveService] Збереження не знайдено. Створюємо нове.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveService] Помилка завантаження: {e.Message}");
            _data = new SaveData();
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(_savePath))
            File.Delete(_savePath);
        _data = new SaveData();
        Debug.Log("[SaveService] Збереження видалено.");
    }

    public string GetSavePath() => _savePath;
}
