using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Assets.Scripts.EventBus
{
    public static class AssemblyScanner
    {
        public static List<Type> GetTypesImplementing<TInterface>() where TInterface : class
        {
            Type interfaceType = typeof(TInterface);

            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"{nameof(TInterface)} must be an interface type.");
            }

            List<Type> types = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    Type[] assemblyTypes = assembly.GetTypes();

                    foreach (Type type in assemblyTypes)
                    {
                        if ((type.IsClass || (type.IsValueType && !type.IsEnum)) &&
                            !type.IsAbstract &&
                            !type.IsInterface &&
                            interfaceType.IsAssignableFrom(type))
                        {
                            types.Add(type);
                        }
                    }
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Debug.LogWarning($"Could not load types from assembly {assembly.FullName}: {ex.Message}. Some event types might be missed.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error scanning assembly {assembly.FullName}: {ex.Message}");
                }
            }

            return types;
        }
    }
}