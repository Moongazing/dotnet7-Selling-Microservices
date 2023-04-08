using Moongazing.EventBus.Base;
using Moongazing.EventBus.Base.Abstraction;
using Moongazing.EventBus.RabbitMQ;
using Moongazing.EventBus.AzureServiceBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Moongazing.EventBus.Base.EventBusConfig;

namespace Moongazing.EventBus.Factory
{
    public static class EventBusFactory
    {
        public static IEventBus Create(EventBusConfig config, IServiceProvider serviceProvider)
        {

            return config.EventBustType switch
            {
                EventBusType.AzureServiceBus => new EventBusServiceBus(config, serviceProvider),
                _ => new EventBusRabbitMQ(config, serviceProvider),
            };
        }
     }
}
