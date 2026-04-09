using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string startScene;

    [Header("Projectile Prefabs (для пулів)")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private GameObject swordPrefab;
    [SerializeField] private GameObject mageOrbPrefab;

    [Header("Pool Sizes")]
    [SerializeField] private int arrowPoolSize   = 30;
    [SerializeField] private int swordPoolSize   = 20;
    [SerializeField] private int mageOrbPoolSize = 20;

    void Awake()
    {
        InitializeServices();
        LoadInitialScene();
    }

    void InitializeServices()
    {
        ServiceLocator.Register(new InputService());
        ServiceLocator.Register(new SceneService());

        var poolService = new PoolService();
        ServiceLocator.Register(poolService);
        InitializePools(poolService);

        ServiceLocator.Register(new SaveService());
    }

    void InitializePools(PoolService poolService)
    {
        if (arrowPrefab   != null) poolService.CreatePool(PoolName.arrow,   arrowPrefab,   arrowPoolSize);
        if (swordPrefab   != null) poolService.CreatePool(PoolName.sword,   swordPrefab,   swordPoolSize);
        if (mageOrbPrefab != null) poolService.CreatePool(PoolName.mageOrb, mageOrbPrefab, mageOrbPoolSize);
    }
    
    void LoadInitialScene()
    {
        var sceneLoader = ServiceLocator.Get<SceneService>();
        sceneLoader.LoadScene(startScene);
        
    }
}
