using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// Завжди запускає гру зі сцени Bootstrap.
/// Після зупинки — повертає сцену, з якої був натиснутий Play.
///
/// Увімкнути/вимкнути: Tools → Play From Bootstrap
/// </summary>
[InitializeOnLoad]
public static class PlayFromBootstrap
{
    private const string BootstrapPath  = "Assets/Scenes/Bootstrap.unity";
    private const string PrevSceneKey   = "PlayFromBootstrap_PrevScene";
    private const string EnabledKey     = "PlayFromBootstrap_Enabled";
    private const string MenuPath       = "Tools/Play From Bootstrap";

    static PlayFromBootstrap()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    // ── увімкнення / вимкнення через меню ────────────────────────────────────

    [MenuItem(MenuPath)]
    private static void ToggleEnabled()
    {
        bool enabled = IsEnabled();
        EditorPrefs.SetBool(EnabledKey, !enabled);
    }

    [MenuItem(MenuPath, validate = true)]
    private static bool ToggleEnabledValidate()
    {
        Menu.SetChecked(MenuPath, IsEnabled());
        return true;
    }

    private static bool IsEnabled() => EditorPrefs.GetBool(EnabledKey, true);

    // ── логіка перемикання сцен ───────────────────────────────────────────────

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (!IsEnabled()) return;

        if (state == PlayModeStateChange.ExitingEditMode)
        {
            var activeScene = EditorSceneManager.GetActiveScene();

            // Якщо вже Bootstrap — нічого не робимо
            if (activeScene.path == BootstrapPath) return;

            // Зберігаємо поточну сцену щоб повернутись після зупинки
            EditorPrefs.SetString(PrevSceneKey, activeScene.path);

            // Пропонуємо зберегти незбережені зміни
            EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

            // Відкриваємо Bootstrap
            EditorSceneManager.OpenScene(BootstrapPath);
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Повертаємо попередню сцену
            string prevScene = EditorPrefs.GetString(PrevSceneKey, "");
            if (!string.IsNullOrEmpty(prevScene))
            {
                EditorSceneManager.OpenScene(prevScene);
                EditorPrefs.DeleteKey(PrevSceneKey);
            }
        }
    }
}
