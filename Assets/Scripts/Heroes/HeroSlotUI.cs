using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI-карточка одного героя в панелі вибору.
/// Відображає стан: розблокований / заблокований / вибраний.
///
/// Ієрархія слоту:
///   HeroSlot (HeroSlotUI)
///   ├── HeroIcon (Image)
///   ├── HeroName (TMP)
///   ├── PriceText (TMP)      — видно тільки якщо заблокований
///   ├── LockedOverlay (GO)   — темний оверлей + іконка замка
///   └── SelectHighlight (GO) — підсвітка при виборі
/// </summary>
public class HeroSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Елементи UI")]
    [SerializeField] private Image             heroIcon;
    [SerializeField] private TextMeshProUGUI   heroName;
    [SerializeField] private TextMeshProUGUI   priceText;
    [SerializeField] private GameObject        lockedOverlay;
    [SerializeField] private GameObject        selectedHighlight;

    public HeroData Data { get; private set; }
    private HeroSelectionUI parentUI;

    public void Init(HeroData data, HeroSelectionUI parent)
    {
        Data = data;
        parentUI = parent;
        if (selectedHighlight) selectedHighlight.SetActive(false);
        Refresh();
    }

    public void Refresh()
    {
        if (Data == null) return;

        bool unlocked = IsUnlocked();

        if (heroIcon)  heroIcon.sprite = Data.icon;
        if (heroName)  heroName.text   = Data.displayName;

        if (lockedOverlay) lockedOverlay.SetActive(!unlocked);

        if (priceText)
        {
            priceText.gameObject.SetActive(!unlocked);
            priceText.text = $"${Data.price}";
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectedHighlight) selectedHighlight.SetActive(selected);
    }

    public bool IsUnlocked()
    {
        if (Data == null) return false;
        if (Data.isDefaultUnlocked) return true;
        return ServiceLocator.TryGet<SaveService>(out var save) && save.IsHeroUnlocked(Data.heroId);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Refresh();
        parentUI?.SelectHero(this);
    }
}
