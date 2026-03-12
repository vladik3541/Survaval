using System.Collections.Generic;
using UnityEngine;

public class PoolService
{
    private Dictionary<string, Queue<GameObject>> pools = new();
    private Dictionary<string, GameObject> prefabs = new();
    
    private Transform poolParent;
    
    public PoolService()
    {
        var go = new GameObject("[Object Pools]");
        poolParent = go.transform;
        Object.DontDestroyOnLoad(go);
    }
    
    public void CreatePool(string poolName, GameObject prefab, int initialSize = 10)
    {
        if (pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool already exists: {poolName}");
            return;
        }
        
        prefabs[poolName] = prefab;
        pools[poolName] = new Queue<GameObject>();
        
        // Створюємо контейнер для цього пулу
        var container = new GameObject($"Pool_{poolName}");
        container.transform.SetParent(poolParent);
        
        // Заповнюємо пул
        for (int i = 0; i < initialSize; i++)
        {
            var obj = Object.Instantiate(prefab, container.transform);
            obj.SetActive(false);
            pools[poolName].Enqueue(obj);
        }
        
        Debug.Log($"Pool created: {poolName} ({initialSize} objects)");
    }
    
    public GameObject Get(string poolName, Vector3 position, Quaternion rotation)
    {
        if (!pools.TryGetValue(poolName, out var pool))
        {
            Debug.LogError($"Pool not found: {poolName}");
            return null;
        }
        
        GameObject obj;
        
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            // Пул порожній — створюємо новий об'єкт
            obj = Object.Instantiate(prefabs[poolName]);
            Debug.Log($"Pool {poolName} expanded");
        }
        
        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        
        return obj;
    }
    
    public void Return(string poolName, GameObject obj)
    {
        if (!pools.ContainsKey(poolName))
        {
            Debug.LogWarning($"Pool not found: {poolName}. Destroying object.");
            Object.Destroy(obj);
            return;
        }
        
        obj.SetActive(false);
        pools[poolName].Enqueue(obj);
    }
    
    // Зручний метод для об'єктів з компонентом Poolable
    public void Return(GameObject obj)
    {
        var poolable = obj.GetComponent<Poolable>();
        if (poolable != null)
        {
            Return(poolable.PoolName, obj);
        }
        else
        {
            Object.Destroy(obj);
        }
    }
}

