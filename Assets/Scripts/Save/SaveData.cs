using System;
using System.Collections.Generic;

/// <summary>
/// Модель даних для JSON-збереження.
/// Зберігає: монети, куплених героїв, рівні прокачки.
/// </summary>
[Serializable]
public class SaveData
{
    public bool initialized = false;
    public int coins = 0;
    public List<string> purchasedHeroIds = new List<string>();
    public List<PowerUpSaveEntry> powerUpLevels = new List<PowerUpSaveEntry>();
}

[Serializable]
public class PowerUpSaveEntry
{
    public string typeName;
    public int level;
}
