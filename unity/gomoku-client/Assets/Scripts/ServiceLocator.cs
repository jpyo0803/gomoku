using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    private static Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void Register<T>(T service)
    {
        Type type = typeof(T);
        if (services.ContainsKey(type))
        {
            services[type] = service; // Update existing service
        }
        else
        {
            services.Add(type, service); // Add new service
        }
    }

    public static T Get<T>()
    {
        Type type = typeof(T);
        if (services.TryGetValue(type, out object service))
        {
            return (T)service;
        }
        throw new Exception($"Service of type {type} not registered.");
    }

    public static void Unregister<T>()
    {
        Type type = typeof(T);
        if (services.ContainsKey(type))
        {
            services.Remove(type);
        }
    }
}