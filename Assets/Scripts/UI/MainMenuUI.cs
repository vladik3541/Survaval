using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Контролер головного меню.
/// Кнопка «Грати» → показує панель вибору героя.
///
/// Ієрархія:
///   MainMenuRoot (MainMenuUI)
///   ├── MainPanel (містить кнопки Start/Quit)
///   │   ├── StartButton (Button)
///   │   └── QuitButton  (Button)
///   └── HeroSelectionPanel (HeroSelectionUI) — спочатку вимкнена
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    [Header("Панелі")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject heroSelectionPanel;

    [Header("Кнопки головного меню")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button quitButton;

    private void Awake()
    {
        startButton?.onClick.AddListener(OnStartClicked);
        quitButton?.onClick.AddListener(OnQuitClicked);
    }

    private void Start()
    {
        ShowMainMenu();
    }

    // ── кнопки ───────────────────────────────────────────────────────────────

    private void OnStartClicked()
    {
        if (heroSelectionPanel) heroSelectionPanel.SetActive(true);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ── публічне API ──────────────────────────────────────────────────────────

    public void ShowMainMenu()
    {
        if (mainPanel)          mainPanel.SetActive(true);
        if (heroSelectionPanel) heroSelectionPanel.SetActive(false);
    }

    /// <summary>Викликається з HeroSelectionUI → кнопка «Назад».</summary>
    public void ReturnFromHeroSelection()
    {
        ShowMainMenu();
    }
}
