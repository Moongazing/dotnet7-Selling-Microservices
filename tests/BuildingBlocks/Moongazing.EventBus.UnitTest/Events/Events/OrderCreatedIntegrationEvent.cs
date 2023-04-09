using Moongazing.EventBus.Base.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moongazing.EventBus.UnitTest.Events.Events
{
    public class OrderCreatedIntegrationEvent:IntegrationEvent
    {
        public int Id { get; set; }
        public OrderCreatedIntegrationEvent(int id)
        {
            Id = id;
        }
    }
}
