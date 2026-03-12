using TMPro;
using UnityEngine;

/// <summary>
/// Відображає кількість монет гравця в головному меню.
/// Читає з SaveService і оновлюється через подію OnCoinsChanged.
/// </summary>
public class MainMenuStats : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI totalGoldText;

    private SaveService _save;

    private void Start()
    {
        if (!ServiceLocator.TryGet<SaveService>(out _save))
        {
            Debug.LogWarning("[MainMenuStats] SaveService не знайдено.");
            return;
        }

        _save.OnCoinsChanged += RefreshGold;
        RefreshGold(_save.Coins);
    }

    private void OnDestroy()
    {
        if (_save != null)
            _save.OnCoinsChanged -= RefreshGold;
    }

    private void RefreshGold(int coins)
    {
        if (totalGoldText != null)
            totalGoldText.text = $"{coins}";
    }
}
