using UnityEngine;

public class GameDataLoader : MonoBehaviour
{
    public static GameDataLoader Instance { get; private set; }

    /// <summary>ID вибраного героя (з PlayerPrefs).</summary>
    public string SelectedHeroId { get; private set; }

    /// <summary>Дані вибраного героя (null якщо не знайдено).</summary>
    public HeroData SelectedHero { get; private set; }

    [Header("Всі доступні герої (перетягни всі HeroData активи)")]
    [SerializeField] private HeroData[] allHeroes;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        LoadSelectedHero();
    }

    private void LoadSelectedHero()
    {
        SelectedHeroId = PlayerPrefs.GetString("SelectedHeroId", "");

        if (string.IsNullOrEmpty(SelectedHeroId))
        {
            Debug.LogWarning("[GameDataLoader] SelectedHeroId не встановлено. Гравець не вибрав героя.");
            return;
        }

        foreach (var hero in allHeroes)
        {
            if (hero != null && hero.heroId == SelectedHeroId)
            {
                SelectedHero = hero;
                Debug.Log($"[GameDataLoader] Завантажено героя: {hero.displayName}");
                return;
            }
        }

        Debug.LogWarning($"[GameDataLoader] Герой з ID '{SelectedHeroId}' не знайдений у списку.");
    }

    // ── доступ до збереження ──────────────────────────────────────────────────

    /// <summary>Поточна кількість монет зі збереження.</summary>
    public int GetCoins()
    {
        return ServiceLocator.TryGet<SaveService>(out var save) ? save.Coins : 0;
    }

    /// <summary>Рівень прокачки вказаного типу зі збереження.</summary>
    public int GetPowerUpLevel(PowerUpType type)
    {
        return ServiceLocator.TryGet<SaveService>(out var save) ? save.GetPowerUpLevel(type) : 0;
    }
}
