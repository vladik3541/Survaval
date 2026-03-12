using System;
using TMPro;
using UnityEngine;

public class MoneyMenuManager : MonoBehaviour
{
    public int startingCoins = 5000;
    public TextMeshProUGUI coinText;
    public static MoneyMenuManager Instance { get; private set; }

    public int Coins { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (ServiceLocator.TryGet<SaveService>(out var save))
        {
            Coins = save.Coins;
        }
        Debug.Log($"[GoldMenuManager] Монети завантажено: {Coins}");
        AddCoins(startingCoins);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;

        if (ServiceLocator.TryGet<SaveService>(out var save))
            save.SetCoins(Coins);
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        RefreshCoins();
    }

    public void RemoveCoins(int amount)
    {
        Coins -= amount;
        RefreshCoins();
    }

    public bool EnoughCoins(int amount)
    {
        return Coins >= amount;
    }
    private void RefreshCoins()
    {
        coinText.text = Coins.ToString();
    }
}
