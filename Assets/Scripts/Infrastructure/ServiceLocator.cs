using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Infrastructure
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> SServices = new Dictionary<Type, object>();

        public static void Register<T>(T serviceInstance) where T : class
        {
            Type type = typeof(T);

            if (!SServices.TryAdd(type, serviceInstance))
            {
                Debug.LogWarning($"[ServiceLocator] Service of type {type.Name} is already registered. Overwriting the existing instance.");

                SServices[type] = serviceInstance;
            }
        }

        public static T Get<T>() where T : class
        {
            Type type = typeof(T);

            if (SServices.TryGetValue(type, out object serviceInstance))
            {
                return (T)serviceInstance;
            }

            Debug.LogError($"[ServiceLocator] Service of type {type.Name} not found.");

            throw new InvalidOperationException($"Service of type {type.Name} has not been registered.");
        }

        public static void Unregister<T>() where T : class
        {
            Type type = typeof(T);

            if (SServices.Remove(type))
            {
                // Debug.Log($"[ServiceLocator] Unregistered service: {type.Name}");
            }
            else
            {
                Debug.LogWarning($"[ServiceLocator] Attempted to unregister service of type {type.Name} which was not registered.");
            }
        }

        public static void Clear()
        {
            SServices.Clear();
        }
    }
}