using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Singleton AudioManager для гри типу Vampire Survivors.
/// Керує всіма SFX через пул AudioSource.
/// 
/// Використання:
///   AudioManager.Instance.PlayPickupXP();
///   AudioManager.Instance.PlayLevelUp();
///   AudioManager.Instance.PlaySwordSwing(transform.position);
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Clips — Підбір")]
    [Tooltip("Підбір кристалу досвіду")]
    public AudioClip pickupXP;
    [Tooltip("Підбір аптечки / відновлення HP")]
    public AudioClip pickupHealth;
    [Tooltip("Підбір золота / монет")]
    public AudioClip pickupGold;

    [Header("Audio Clips — Бій")]
    [Tooltip("Знищення ворога (death)")]
    public AudioClip enemyKill;
    [Tooltip("Гравець отримує урон")]
    public AudioClip playerHit;

    [Header("Audio Clips — Прогресія")]
    [Tooltip("Підвищення рівня гравця")]
    public AudioClip levelUp;

    [Header("Audio Clips — Зброя")]
    [Tooltip("Удар мечем / swing")]
    public AudioClip swordSwing;
    [Tooltip("Снаряд мага / fireball")]
    public AudioClip mageProjectile;
    [Tooltip("Постріл стріли лучника")]
    public AudioClip arrowShoot;
    [Tooltip("Запуск ракети")]
    public AudioClip rocketLaunch;
    [Tooltip("Вибух ракети при зіткненні з землею")]
    public AudioClip rocketImpact;

    [Header("Налаштування гучності")]
    [Range(0f, 1f)] public float masterVolume   = 1f;
    [Range(0f, 1f)] public float sfxVolume       = 1f;
    [Range(0f, 1f)] public float pickupVolume    = 0.8f;
    [Range(0f, 1f)] public float weaponVolume    = 0.9f;
    [Range(0f, 1f)] public float uiVolume        = 1f;

    [Header("Пул AudioSource")]
    [Tooltip("Скільки AudioSource тримати в пулі (більше = більше одночасних звуків)")]
    [Range(8, 32)] public int poolSize = 16;

    // ── Spatial sound settings ────────────────────────────────────────────────
    [Header("Просторовий звук")]
    public bool useSpatialAudio = false;
    [Range(1f, 50f)] public float spatialMaxDistance = 20f;

    // ── Internal ──────────────────────────────────────────────────────────────
    private readonly Queue<AudioSource> _pool = new Queue<AudioSource>();
    private Transform _poolParent;

    // ─────────────────────────────────────────────────────────────────────────
    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildPool();
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Pool

    private void BuildPool()
    {
        _poolParent = new GameObject("_AudioPool").transform;
        _poolParent.SetParent(transform);

        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"SFX_{i:D2}");
            go.transform.SetParent(_poolParent);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = useSpatialAudio ? 1f : 0f;
            if (useSpatialAudio) src.maxDistance = spatialMaxDistance;
            _pool.Enqueue(src);
        }
    }

    private AudioSource GetSource()
    {
        // Rotate pool: якщо всі зайняті — беремо найстаріший
        var src = _pool.Dequeue();
        if (src.isPlaying) src.Stop();
        _pool.Enqueue(src);
        return src;
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Core Play

    /// <summary>
    /// Відтворити кліп з позицією (для просторового звуку).
    /// position = null → 2D звук (UI / глобальний ефект).
    /// </summary>
    private void Play(AudioClip clip, float volume, Vector3? position = null,
                      float pitch = 1f, float pitchVariance = 0f)
    {
        if (clip == null)
        {
            Debug.LogWarning($"[AudioManager] Clip is NULL! Перевір призначення у Inspector.");
            return;
        }

        float finalVolume = volume * masterVolume * sfxVolume;
        if (finalVolume <= 0f) return;

        var src = GetSource();

        if (position.HasValue && useSpatialAudio)
        {
            src.transform.position = position.Value;
            src.spatialBlend = 1f;
        }
        else
        {
            src.spatialBlend = 0f;
        }

        src.pitch = pitch + Random.Range(-pitchVariance, pitchVariance);
        src.volume = finalVolume;
        src.clip = clip;
        src.Play();
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Public API — Підбір

    /// <summary>Підбір кристалу досвіду (XP)</summary>
    public void PlayPickupXP(Vector3? position = null)
        => Play(pickupXP, pickupVolume, position, pitch: 1f, pitchVariance: 0.05f);

    /// <summary>Підбір аптечки / відновлення здоров'я</summary>
    public void PlayPickupHealth(Vector3? position = null)
        => Play(pickupHealth, pickupVolume, position, pitch: 1f, pitchVariance: 0.03f);

    /// <summary>Підбір золота або монет</summary>
    public void PlayPickupGold(Vector3? position = null)
        => Play(pickupGold, pickupVolume, position, pitch: 1f, pitchVariance: 0.08f);

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Public API — Бій

    /// <summary>Знищення ворога</summary>
    public void PlayEnemyKill(Vector3? position = null)
        => Play(enemyKill, weaponVolume, position, pitch: 1f, pitchVariance: 0.1f);

    /// <summary>Гравець отримує урон</summary>
    public void PlayPlayerHit(Vector3? position = null)
        => Play(playerHit, sfxVolume, position, pitch: 1f, pitchVariance: 0.05f);

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Public API — Прогресія

    /// <summary>Підвищення рівня — рекомендується 2D (UI)</summary>
    public void PlayLevelUp()
        => Play(levelUp, uiVolume, position: null, pitch: 1f);

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Public API — Зброя

    /// <summary>Удар мечем / swing</summary>
    public void PlaySwordSwing(Vector3? position = null)
        => Play(swordSwing, weaponVolume, position, pitch: 1f, pitchVariance: 0.07f);

    /// <summary>Снаряд мага (fireball, ice bolt тощо)</summary>
    public void PlayMageProjectile(Vector3? position = null)
        => Play(mageProjectile, weaponVolume, position, pitch: 1f, pitchVariance: 0.06f);

    /// <summary>Постріл стріли лучника</summary>
    public void PlayArrowShoot(Vector3? position = null)
        => Play(arrowShoot, weaponVolume, position, pitch: 1f, pitchVariance: 0.05f);

    /// <summary>Запуск ракети</summary>
    public void PlayRocketLaunch(Vector3? position = null)
        => Play(rocketLaunch, weaponVolume, position, pitch: 1f, pitchVariance: 0.04f);

    /// <summary>Вибух ракети при зіткненні з землею</summary>
    public void PlayRocketImpact(Vector3? position = null)
        => Play(rocketImpact, weaponVolume, position, pitch: 1f, pitchVariance: 0.06f);

    #endregion

    // ─────────────────────────────────────────────────────────────────────────
    #region Runtime Volume Control

    public void SetMasterVolume(float value)  => masterVolume  = Mathf.Clamp01(value);
    public void SetSFXVolume(float value)     => sfxVolume     = Mathf.Clamp01(value);

    #endregion
}
