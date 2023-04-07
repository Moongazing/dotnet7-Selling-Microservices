using Microsoft.Extensions.DependencyInjection;
using Moongazing.EventBus.Base.Abstraction;
using Moongazing.EventBus.Base.SubManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moongazing.EventBus.Base.Event
{
    public abstract class BaseEventBus : IEventBus
    {
        public readonly IServiceProvider ServiceProvider;
        public readonly IEventBusSubscriptionManager SubsManager;

        private EventBusConfig eventBusConfig;

        public BaseEventBus(EventBusConfig config, IServiceProvider serviceProvider)
        {
            eventBusConfig = config;
            ServiceProvider = serviceProvider;
            SubsManager = new InMemoryEventBusSubscriptionManager(ProcessEventName);
        }
        public virtual string ProcessEventName(string eventName)
        {
            if (eventBusConfig.DeleteEventPrefix)
            {
                eventName = eventName.TrimStart(eventBusConfig.EventNamePrefix.ToArray());
            }
            if (eventBusConfig.DeleteEventPrefix)
            {
                eventName = eventName.TrimEnd(eventBusConfig.EventNamePrefix.ToArray());
            }
            return eventName;
        }
        public virtual string GetSubName(string eventName)
        {
            return $"{eventBusConfig.SubscriberClientAppName}.{ProcessEventName(eventName)}";
        }
        public virtual void Dispose()
        {
            eventBusConfig = null;
        }
        public async Task<bool> ProcessEvent(string eventName, string message)
        {
            eventName = ProcessEventName(eventName);

            var processed = false;
            if (SubsManager.HasSubsriptionsForEvent(eventName))
            {

                var subscriptions = SubsManager.GetHandlersForEvent(eventName);
                using (var scope = ServiceProvider.CreateScope())
                {
                    foreach (var subscription in subscriptions)
                    {
                        var handler = ServiceProvider.GetService(subscription.HandlerType);
                        if (handler == null) continue;


                        var eventType = SubsManager.GetEventTypeByName($"{eventBusConfig.EventNamePrefix}{eventName}{eventBusConfig.EventNameSuffix}");
                        var integrationEvent = JsonConvert.DeserializeObject(message, eventType);
                        

                        var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                        await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
                    }
                }
                processed = true;
            }
            return processed;
        }

        public abstract void Publish(IntegrationEvent @event);
        

        public abstract void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        

        public abstract void UnSubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>();
        
    }
}
