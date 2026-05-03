using System;

namespace Assets.Scripts.EventBus
{
    public interface IEventBinding<in T> where T : IEvent
    {
        public Action<T> OnEvent { get; }
        public object Owner { get; }
    }
}