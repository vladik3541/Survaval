using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class SceneService
{
    public event Action<string> OnSceneLoadStarted;
    public event Action<string> OnSceneLoaded;
    public event Action<float> OnLoadProgress;
    
    public string CurrentSceneName => SceneManager.GetActiveScene().name;
    public bool IsLoading { get; private set; }
    
    private MonoBehaviour coroutineRunner;
    public SceneService()
    {
        var go = new GameObject("[SceneService]");
        coroutineRunner = go.AddComponent<CoroutineRunner>();
        Object.DontDestroyOnLoad(go);
    }
    public void LoadScene(string sceneName)
    {
        if (IsLoading) return;
        coroutineRunner.StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    public void LoadSceneWithLoadingScreen(string sceneName, string loadingSceneName = "Loading")
    {
        if (IsLoading) return;
        coroutineRunner.StartCoroutine(LoadWithLoadingScreen(sceneName, loadingSceneName));
    }
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        IsLoading = true;
        OnSceneLoadStarted?.Invoke(sceneName);
        
        var operation = SceneManager.LoadSceneAsync(sceneName);
        
        while (!operation.isDone)
        {
            OnLoadProgress?.Invoke(operation.progress);
            yield return null;
        }
        
        IsLoading = false;
        OnSceneLoaded?.Invoke(sceneName);
    }
    
    private IEnumerator LoadWithLoadingScreen(string targetScene, string loadingScene)
    {
        IsLoading = true;
        
        // Завантажуємо Loading сцену
        yield return SceneManager.LoadSceneAsync(loadingScene);
        
        // Даємо кадр на ініціалізацію Loading UI
        yield return null;
        
        OnSceneLoadStarted?.Invoke(targetScene);
        
        // Завантажуємо цільову сцену
        var operation = SceneManager.LoadSceneAsync(targetScene);
        operation.allowSceneActivation = false;
        
        while (operation.progress < 0.9f)
        {
            OnLoadProgress?.Invoke(operation.progress);
            yield return null;
        }
        
        // Мінімальний час показу loading screen
        yield return new WaitForSeconds(0.5f);
        
        operation.allowSceneActivation = true;
        
        IsLoading = false;
        OnSceneLoaded?.Invoke(targetScene);
    }
    
    public void ReloadCurrentScene()
    {
        LoadScene(CurrentSceneName);
    }
}
// Допоміжний клас для запуску корутин
public class CoroutineRunner : MonoBehaviour { }