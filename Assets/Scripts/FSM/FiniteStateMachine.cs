using System;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.FSM
{
    public class FiniteStateMachine<TContext> : IFiniteStateMachine<TContext> where TContext : class
    {
        public IState<TContext> CurrentState { get; private set; }
        public IState<TContext> PreviousState { get; private set; }

        public event Action<IState<TContext>> StateChanged;

        private bool _isLocked;
        private Coroutine _stateChangeCoroutine;
        private readonly MonoBehaviour _coroutineRunner;

        public FiniteStateMachine(MonoBehaviour coroutineRunner)
        {
            _coroutineRunner = coroutineRunner ?? throw new ArgumentNullException(nameof(coroutineRunner),
                "MonoBehaviour runner cannot be null for FSM operations requiring coroutines.");
        }

        public void Initialize(IState<TContext> initialState)
        {
            if (initialState == null)
            {
                throw new ArgumentNullException(nameof(initialState), "Initial state cannot be null.");
            }

            if (CurrentState != null)
            {
                Debug.LogWarning("[FSM] FSM is already initialized. Ignoring call.");
                return;
            }

            CurrentState = initialState;
            CurrentState.Enter();

            StateChanged?.Invoke(CurrentState);
        }

        public void ChangeState(IState<TContext> newState, bool forceChange = false)
        {
            if (newState == null)
            {
                Debug.LogError("[FSM] Cannot change to a null state.");
                return;
            }

            if (!forceChange)
            {
                if (_isLocked)
                {
                    Debug.LogWarning($"[FSM] State change blocked for {newState.GetType().Name}. FSM is locked.");
                    return;
                }

                if (newState == CurrentState)
                {
                    return;
                }
            }

            StopDelayedStateChangeInternal();
            PerformStateChange(newState);
        }

        public void ChangeStateWithDelay(IState<TContext> newState, float delay, bool forceChange = false)
        {
            if (newState == null)
            {
                Debug.LogError("[FSM] Cannot change to a null state with delay.");
                return;
            }

            if (delay <= 0)
            {
                Debug.LogWarning($"[FSM] Delay is zero or negative ({delay}s). Changing state immediately.");

                ChangeState(newState, forceChange);
                return;
            }

            if (!forceChange)
            {
                if (_isLocked)
                {
                    Debug.LogWarning($"[FSM] Delayed state change blocked for {newState.GetType().Name}. FSM is locked.");
                    return;
                }

                if (newState == CurrentState)
                {
                    return;
                }
            }

            if (_stateChangeCoroutine != null)
            {
                if (!forceChange)
                {
                    Debug.LogWarning($"[FSM] Another delayed state change is already pending. Ignoring request.");
                    return;
                }

                StopDelayedStateChangeInternal();
            }

            _stateChangeCoroutine = _coroutineRunner.StartCoroutine(DoChangeStateWithDelay(newState, delay, forceChange));
        }

        public void StopDelayedStateChange()
        {
            StopDelayedStateChangeInternal();
        }

        public void Lock() => _isLocked = true;
        public void Unlock() => _isLocked = false;

        private void PerformStateChange(IState<TContext> newState)
        {
            IState<TContext> oldState = CurrentState;

            try
            {
                oldState?.Exit();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FSM] Exception during Exit of state {oldState?.GetType().Name}: {ex}");
            }

            PreviousState = oldState;
            CurrentState = newState;

            try
            {
                CurrentState.Enter();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FSM] Exception during Enter of state {CurrentState?.GetType().Name}: {ex}");
            }

            try
            {
                StateChanged?.Invoke(CurrentState);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FSM] Exception during StateChanged event: {ex}");
            }
        }

        private IEnumerator DoChangeStateWithDelay(IState<TContext> newState, float delay, bool forceChange)
        {
            yield return new WaitForSeconds(delay);

            if (!forceChange && _isLocked)
            {
                _stateChangeCoroutine = null;
                yield break;
            }

            if (newState != CurrentState || forceChange)
            {
                PerformStateChange(newState);
            }

            _stateChangeCoroutine = null;
        }

        private void StopDelayedStateChangeInternal()
        {
            if (_stateChangeCoroutine != null)
            {
                if (_coroutineRunner) _coroutineRunner.StopCoroutine(_stateChangeCoroutine);

                _stateChangeCoroutine = null;
            }
        }
    }
}