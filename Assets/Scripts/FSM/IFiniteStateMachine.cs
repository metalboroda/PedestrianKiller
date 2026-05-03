using System;

namespace Assets.Scripts.FSM
{
    public interface IFiniteStateMachine<TContext> where TContext : class
    {
        IState<TContext> CurrentState { get; }
        IState<TContext> PreviousState { get; }

        public event Action<IState<TContext>> StateChanged;

        public void Initialize(IState<TContext> initialState);

        public void ChangeState(IState<TContext> newState, bool forceChange = false);

        public void ChangeStateWithDelay(IState<TContext> newState, float delay, bool forceChange = false);

        public void StopDelayedStateChange();

        public void Lock();

        public void Unlock();
    }
}