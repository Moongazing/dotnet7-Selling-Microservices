using Moongazing.EventBus.Base.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moongazing.EventBus.Base.Abstraction
{
    public interface IEventBusSubsriptionManager
    {
        bool IsEmpty { get; }
        event EventHandler<string> OnEventRemoved;
        void AddSubscription<T,TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>;
        void RemoveSubscription<T,TH>() where TH : IIntegrationEventHandler<T> where T: IntegrationEvent;\
        bool HasSubsriptionsForEvent<T>() where T : IntegrationEvent;
        bool HasSubsriptionsForEvent(string eventName);
        Type GetEventType(string eventName);
        void Clear();
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>() where T : IntegrationEvent;
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName);
        string GetEventKey<T>();
    }
}
