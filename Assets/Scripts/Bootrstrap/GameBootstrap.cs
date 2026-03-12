using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private string startScene;
    void Awake()
    {
        InitializeServices();
        LoadInitialScene();
    }
    
    void InitializeServices()
    {
        ServiceLocator.Register(new InputService());
        ServiceLocator.Register(new SceneService());
        ServiceLocator.Register(new PoolService());
        ServiceLocator.Register(new SaveService());
    }
    
    void LoadInitialScene()
    {
        var sceneLoader = ServiceLocator.Get<SceneService>();
        sceneLoader.LoadScene(startScene);
    }
}
