using System;

namespace Assets.Scripts.FSM
{
    public abstract class State<TContext> : IState<TContext> where TContext : class
    {
        protected TContext Context { get; private set; }

        public virtual void Setup(TContext context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate() { }
    }
}