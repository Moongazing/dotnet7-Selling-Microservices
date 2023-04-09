using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moongazing.EventBus.Base;
using Moongazing.EventBus.Base.Abstraction;
using Moongazing.EventBus.Factory;
using Moongazing.EventBus.UnitTest.Events.EventHandlers;
using Moongazing.EventBus.UnitTest.Events.Events;
using RabbitMQ.Client;
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

        [TestMethod]
        public void subscribe_event_on_rabbitmq_test()
        {
            _services.AddSingleton<IEventBus>(sp =>
            {
                EventBusConfig config = new EventBusConfig()
                {
                    ConnectionRetryCount = 3,
                    SubscriberClientAppName = "EventBus.UnitTest",
                    DefaultTopicName = "MoongazingEventBus",
                    EventBustType = EventBusType.RabbitMQ,
                    EventNameSuffix = "IntegrationEvent",
                    //Connection = new ConnectionFactory
                    //{
                    //    HostName = "localhost",
                    //    Port = 5672,
                    //    UserName = "guest",
                    //    Password = "guest",

                    //}
                };
                return EventBusFactory.Create(config, sp);
            });
            var serviceProvider = _services.BuildServiceProvider();

            var eventBus = serviceProvider.GetRequiredService<IEventBus>();
            eventBus.Subscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();
            eventBus.UnSubscribe<OrderCreatedIntegrationEvent, OrderCreatedIntegrationEventHandler>();

        }
    }
}