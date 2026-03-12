using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    private static readonly Dictionary<Type, object> services = new();
    
    public static void Register<T>(T service) where T : class
    {
        var type = typeof(T);
        
        if (services.ContainsKey(type))
        {
            Debug.LogWarning($"Service {type.Name} already registered. Replacing.");
        }
        
        services[type] = service;
        Debug.Log($"Service registered: {type.Name}");
    }
    
    public static T Get<T>() where T : class
    {
        var type = typeof(T);
        
        if (services.TryGetValue(type, out var service))
        {
            return (T)service;
        }
        
        Debug.LogError($"Service not found: {type.Name}");
        return null;
    }
    
    public static bool TryGet<T>(out T service) where T : class
    {
        var type = typeof(T);
        
        if (services.TryGetValue(type, out var obj))
        {
            service = (T)obj;
            return true;
        }
        
        service = null;
        return false;
    }
    
    public static void Clear()
    {
        services.Clear();
    }
}