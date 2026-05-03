using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.FSM
{
    public class StateFactory<TContext> where TContext : class
    {
        private readonly Dictionary<Type, IState<TContext>> _states = new Dictionary<Type, IState<TContext>>();
        private readonly TContext _context;

        public StateFactory(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public TState GetState<TState>() where TState : class, IState<TContext>, new()
        {
            Type type = typeof(TState);

            if (!_states.TryGetValue(type, out IState<TContext> state))
            {
                try
                {
                    state = new TState();
                    state.Setup(_context);

                    _states[type] = state;
                }
                catch (Exception ex)
                {
                    Debug.LogError(
                        $"[StateFactory] Failed to create state of type {type.Name}. Ensure it has a parameterless constructor and Setup method works correctly. Error: {ex}");

                    throw new InvalidOperationException($"Failed to create state of type {type.Name}.", ex);
                }
            }

            return (TState)state;
        }

        public TState GetState<TState>(Func<TState> factoryMethod) where TState : class, IState<TContext>
        {
            if (factoryMethod == null)
            {
                throw new ArgumentNullException(nameof(factoryMethod));
            }

            Type type = typeof(TState);

            if (!_states.TryGetValue(type, out IState<TContext> state))
            {
                try
                {
                    state = factoryMethod();

                    if (state == null)
                    {
                        throw new InvalidOperationException($"Factory method for state {type.Name} returned null.");
                    }

                    state.Setup(_context);

                    _states[type] = state;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[StateFactory] Failed to create state of type {type.Name} using factory method. Error: {ex}");

                    throw new InvalidOperationException($"Failed to create state of type {type.Name} using factory method.", ex);
                }
            }

            return (TState)state;
        }

        public void ClearCache()
        {
            _states.Clear();
        }
    }
}