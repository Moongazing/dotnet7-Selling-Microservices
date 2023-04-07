using Moongazing.EventBus.Base.Abstraction;
using Moongazing.EventBus.Base.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moongazing.EventBus.Base.SubManager
{
    public class InMemoryEventBusSubscriptionManager : IEventBusSubscriptionManager
    {
        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;

        public event EventHandler<string> OnEventRemoved;
        public Func<string, string> eventNameGetter;
        public InMemoryEventBusSubscriptionManager(Func<string, string> eventNameGetter)
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
            this.eventNameGetter = eventNameGetter;
        }
        public bool IsEmpty => !_handlers.Any();
        public void Clear() => _handlers.Clear();
        public void AddSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            AddSubscription(typeof(TH), eventName);

            if(!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
        }
        private void AddSubscription(Type handlerType,string eventName)
        {
            if (!HasSubsriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }
            if (_handlers[eventName].Any(x=>x.HandlerType == handlerType))
            {
                throw new ArgumentException($"Handler type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }
            _handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));

        }
        public string GetEventKey<T>()
        {
            string eventName = typeof(T).Name;
            return eventNameGetter(eventName);
        }
        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(x => x.Name == eventName);
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return GetHandlersForEvent(key);
        }
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName) => _handlers[eventName];
        private void RaiseOnEventRemoved(string eventName)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(this, eventName);
        }
        private SubscriptionInfo FindSubsriptionToRemove<T, TH>() where T: IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            var eventName = GetEventKey<T>();
            return FindSubsriptionToRemove(eventName, typeof(TH));
        }
        private SubscriptionInfo FindSubsriptionToRemove(string eventName,Type handlerType)
        {
            if (!HasSubsriptionsForEvent(eventName))
            {
                return null;
            }
            return _handlers[eventName].SingleOrDefault(x => x.HandlerType == handlerType);
        }
        public bool HasSubsriptionsForEvent<T>() where T : IntegrationEvent
        {
            var key = GetEventKey<T>();
            return HasSubsriptionsForEvent(key);
        }
        public bool HasSubsriptionsForEvent(string eventName) => _handlers.ContainsKey(eventName);
        public void RemoveSubscription<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            throw new NotImplementedException();
        }
    }
}
