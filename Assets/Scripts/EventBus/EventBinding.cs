using System;

namespace Assets.Scripts.EventBus
{
    public class EventBinding<T> : IEventBinding<T> where T : IEvent
    {
        public Action<T> OnEvent => _onEvent;
        public object Owner => _owner;

        private Action<T> _onEvent;
        private readonly object _owner;

        public EventBinding(Action<T> onEvent, object owner = null)
        {
            _onEvent = onEvent ?? throw new ArgumentNullException(nameof(onEvent));
            
            _owner = owner;
        }

        public EventBinding(Action onEventNoArgs, object owner = null)
        {
            if (onEventNoArgs == null) throw new ArgumentNullException(nameof(onEventNoArgs));

            _onEvent = _ => onEventNoArgs();
            
            _owner = owner;
        }

        public void Add(Action<T> onEvent) => _onEvent += onEvent ?? throw new ArgumentNullException(nameof(onEvent));
        public void Remove(Action<T> onEvent) => _onEvent -= onEvent;
        public void ClearEvent() => _onEvent = _ => { };
    }
}