using Moongazing.EventBus.Base.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Moongazing.EventBus.Base.Abstraction
{
    public interface IIntegrationEventHandler<TIntegrationEvent>: IntegrationEventHandler where TIntegrationEvent:IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);

    }
    public interface IntegrationEventHandler
    {

    }
}
