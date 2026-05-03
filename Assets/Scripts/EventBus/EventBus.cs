using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.EventBus
{
    public static class EventBus<T> where T : IEvent
    {
        private static readonly ConcurrentDictionary<IEventBinding<T>, byte> SBindings = new();

        public static void Register(IEventBinding<T> binding)
        {
            if (binding == null) throw new ArgumentNullException(nameof(binding));

            SBindings.TryAdd(binding, 0);
        }

        public static void Unregister(IEventBinding<T> binding)
        {
            if (binding == null) return;

            SBindings.TryRemove(binding, out _);
        }

        public static void Raise(T eventData)
        {
            List<IEventBinding<T>> bindingsSnapshot = new List<IEventBinding<T>>(SBindings.Keys);

            foreach (IEventBinding<T> binding in bindingsSnapshot)
            {
                try
                {
                    binding.OnEvent?.Invoke(eventData);
                }
                catch (Exception)
                {
                    // Debug.LogError($"Error executing event handler for {typeof(T).Name}: {e}");
                }
            }
        }

        internal static void Clear()
        {
            SBindings.Clear();
        }
    }
}