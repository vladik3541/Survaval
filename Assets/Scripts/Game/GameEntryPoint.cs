using UnityEngine;
public class GameEntryPoint : MonoBehaviour
{
    [SerializeField] private Transform        spawnPoint;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private SpawnManager spawnManager;

    private void Start()
    {
        var hero = GameDataLoader.Instance?.SelectedHero;

        if (hero == null)
        {
            Debug.LogError("[GameEntryPoint] SelectedHero не знайдено. Перевір GameDataLoader та PlayerPrefs.");
            return;
        }

        if (hero.characterPrefab == null)
        {
            Debug.LogError($"[GameEntryPoint] У героя '{hero.displayName}' не призначений characterPrefab.");
            return;
        }

        var position = spawnPoint != null ? spawnPoint.position : Vector3.zero;
        var player   = Instantiate(hero.characterPrefab, position, Quaternion.identity);

        // Застосовуємо всі стати з HeroData + бонуси PowerUp через PlayerStats
        player.GetComponent<PlayerStats>()?.Initialize(hero);

        cameraController?.Initialize(player.transform);
        spawnManager?.Initialize(player.transform);

        // Передаємо PlayerHealth до HUD — підписка на події відбувається всередині
        var playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            HUDManager.Instance?.Initialize(playerHealth);
        else
            Debug.LogWarning($"[GameEntryPoint] PlayerHealth не знайдено на '{hero.displayName}'.");

        Debug.Log($"[GameEntryPoint] Заспавнено: {hero.displayName} | HP:{hero.baseHealth} SPD:{hero.baseSpeed} RAD:{hero.basePickupRadius}");
    }
}
