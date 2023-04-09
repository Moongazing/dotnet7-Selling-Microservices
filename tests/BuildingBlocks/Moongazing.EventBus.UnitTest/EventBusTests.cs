using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moongazing.EventBus.Base;
using Moongazing.EventBus.Base.Abstraction;
using Moongazing.EventBus.Factory;
using Moongazing.EventBus.UnitTest.Events.EventHandlers;
using Moongazing.EventBus.UnitTest.Events.Events;
using RabbitMQ.Client;
using System.Net;
using static Moongazing.EventBus.Base.EventBusConfig;

namespace Moongazing.EventBus.UnitTest
{
    [TestClass]
    public class EventBusTests
    {
        private ServiceCollection _services;

        public EventBusTests()
        {
            _services = new ServiceCollection();
            _services.AddLogging(configure => configure.AddConsole());
        }
        private EventBusConfig GetAzureConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 3,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "EventBus1",
                EventBustType = EventBusType.AzureServiceBus,
                EventNameSuffix = "IntegrationEvent",
                EventBusConnectionString = @"Endpoint = sb://moongazing.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=qhflpPem4I2E1OAPwJ+9wrIMbnGuu3oQ4+ASbI6VbSw="
            };

        }
        private EventBusConfig GetRabbitMQConfig()
        {
            return new EventBusConfig()
            {
                ConnectionRetryCount = 3,
                SubscriberClientAppName = "EventBus.UnitTest",
                DefaultTopicName = "EventBus1",
                EventBustType = EventBusType.RabbitMQ,
                EventNameSuffix = "IntegrationEvent",

            };

        }

        [TestMethod]
        public void subscribe_event_on_rabbitmq_test()
        {
            _services.AddSingleton<IEventBus>(sp =>
            {
               
                return EventBusFactory.Create(GetRabbitMQConfig(), sp);
            });
            var serviceProvider = _services.BuildServiceProvider();

            var eventBus = serviceProvider.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
           // eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

        }
        [TestMethod]
        public void subscribe_event_on_azure_test()
        {
            _services.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetAzureConfig(), sp);
            });
            var serviceProvider = _services.BuildServiceProvider();

            var eventBus = serviceProvider.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

        }
  

        [TestMethod]
        public void send_message_to_rabbitmq_test()
        {
            _services.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetRabbitMQConfig(), sp);
            });
            var serviceProvider = _services.BuildServiceProvider();

            var eventBus = serviceProvider.GetRequiredService<IEventBus>();

            eventBus.Publish(new OrderCreatedIntegrationEvent(1));


        }
        [TestMethod]
        public void send_message_to_azure_test()
        {
            _services.AddSingleton<IEventBus>(sp =>
            {
                return EventBusFactory.Create(GetAzureConfig(), sp);
            });
            var serviceProvider = _services.BuildServiceProvider();

            var eventBus = serviceProvider.GetRequiredService<IEventBus>();

            eventBus.Publish(new OrderCreatedIntegrationEvent(1));
        }
       
       
    }
}