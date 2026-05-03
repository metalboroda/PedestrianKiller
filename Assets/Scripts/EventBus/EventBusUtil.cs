using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Assets.Scripts.EventBus
{
    public static class EventBusUtil
    {
        private static IReadOnlyList<Type> _sEventTypes;
        private static List<Action> _sClearActions;

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        public static void InitializeEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                ClearAllBuses();
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void InitializeRuntime()
        {
            _sEventTypes = AssemblyScanner.GetTypesImplementing<IEvent>();

            InitializeAllBuses();
        }

        private static void InitializeAllBuses()
        {
            List<Type> busTypes = new List<Type>();
            List<Action> clearActions = new List<Action>();
            Type genericBusTypeDefinition = typeof(EventBus<>);

            if (_sEventTypes == null)
            {
                Debug.LogError("Event types list is null during bus initialization.");

                _sClearActions = clearActions;
                return;
            }

            foreach (Type eventType in _sEventTypes)
            {
                try
                {
                    Type specificBusType = genericBusTypeDefinition.MakeGenericType(eventType);

                    busTypes.Add(specificBusType);

                    MethodInfo clearMethod = specificBusType.GetMethod(
                        "Clear",
                        BindingFlags.Static | BindingFlags.NonPublic
                    );

                    if (clearMethod != null)
                    {
                        Action clearAction = (Action)Delegate.CreateDelegate(typeof(Action), clearMethod);

                        clearActions.Add(clearAction);
                    }
                    else
                    {
                        Debug.LogWarning($"Could not find Clear method on EventBus<{eventType.Name}>");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to initialize EventBus for type {eventType.Name}: {ex.Message}");
                }
            }

            _sClearActions = clearActions;
        }

        public static void ClearAllBuses()
        {
            if (_sClearActions == null)
            {
                Debug.LogWarning("Clear actions cache is not initialized. Cannot clear buses.");
                return;
            }

            foreach (Action clearAction in _sClearActions)
            {
                try
                {
                    clearAction?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error clearing an event bus: {e}");
                }
            }
        }
    }
}