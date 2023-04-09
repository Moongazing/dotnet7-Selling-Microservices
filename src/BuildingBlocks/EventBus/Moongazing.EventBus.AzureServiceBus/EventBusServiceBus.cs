using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.Extensions.Logging;
using Moongazing.EventBus.Base;
using Moongazing.EventBus.Base.Event;
using Newtonsoft.Json;
using System.Text;

namespace Moongazing.EventBus.AzureServiceBus
{
    public class EventBusServiceBus : BaseEventBus
    {
        private ITopicClient _topicClient;
        private ManagementClient _managmentClient;
        private ILogger _logger; 

        public EventBusServiceBus(EventBusConfig config, IServiceProvider serviceProvider) : base(config, serviceProvider)
        {
            _logger = serviceProvider.GetService(typeof(ILogger<EventBusServiceBus>)) as ILogger<EventBusServiceBus>;
            _managmentClient = new ManagementClient(config.EventBusConnectionString);
            _topicClient = CreateTopicClient();

        }
        private ITopicClient CreateTopicClient()
        {
            if (_topicClient == null || _topicClient.IsClosedOrClosing)
            {
                _topicClient = new TopicClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, RetryPolicy.Default);
            }
            if (!_managmentClient.TopicExistsAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult())
            {
                _managmentClient.CreateTopicAsync(EventBusConfig.DefaultTopicName).GetAwaiter().GetResult();
            }
            return _topicClient;

        }
        public override void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;

            eventName = ProcessEventName(eventName);

            var eventStr = JsonConvert.SerializeObject(@event);
            var bodyArray = Encoding.UTF8.GetBytes(eventStr);

            var message = new Message()
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = bodyArray,
                Label = eventName,

            };

            _topicClient.SendAsync(message).GetAwaiter().GetResult();
        }
        public override void Subscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            eventName = ProcessEventName(eventName);
            if (!SubsManager.HasSubsriptionsForEvent(eventName))
            {
                var subsClient = CreateSubscriptionClientIfNotExists(eventName);

                RegisterSubscriptionClientMessageHandler(subsClient);
            }
            _logger.LogInformation("Subscribing to event{EventName} with {EventHandler}",eventName,typeof(TH).Name);
            SubsManager.AddSubscription<T, TH>();
        }

        public override void UnSubscribe<T, TH>()
        {
            var eventName = typeof(T).Name;
            try
            {
                var subsClient = CreateSubscriptionClient(eventName);
                subsClient
                    .RemoveRuleAsync(eventName)
                    .GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {

                _logger.LogWarning("The messaging entity {EventName} could not be found.", eventName);
            }
            _logger.LogInformation("Unsubscribing from event { EventName }", eventName);
            SubsManager.RemoveSubscription<T, TH>();
        }

        private void RegisterSubscriptionClientMessageHandler(ISubscriptionClient subscriptionClient)
        {
            subscriptionClient.RegisterMessageHandler(
                async (message, token) =>
            {
                var eventName = $"{message.Label}";
                var messageData = Encoding.UTF8.GetString(message.Body);

                if (await ProcessEvent(ProcessEventName(eventName), messageData))
                {
                    await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);
                }
            },
                new MessageHandlerOptions(ExceptionReceivedHandler)
                {
                    MaxConcurrentCalls=10,
                    AutoComplete = false
                });
        }

        private Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            var ex= exceptionReceivedEventArgs.Exception;
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;

            _logger.LogError(ex, "ERROR handling message:{ExceptionMessage} - Context:{@ExceptionContext}", ex.Message, context);

            return Task.CompletedTask;
        }
        private ISubscriptionClient CreateSubscriptionClientIfNotExists(string eventName)
        {
            var subClient = CreateSubscriptionClient(eventName);
            var exists = _managmentClient.SubscriptionExistsAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();

            if (!exists)
            {
                _managmentClient.CreateSubscriptionAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName)).GetAwaiter().GetResult();
                RemoveDefaultRule(subClient);
            }
            CreateRuleIfNotExists(ProcessEventName(eventName), subClient);
            return subClient;
        }
        private void CreateRuleIfNotExists(string eventName, ISubscriptionClient subscriptionClient)
        {
            bool ruleExists;

            try
            {
                var rule = _managmentClient.GetRuleAsync(EventBusConfig.DefaultTopicName, GetSubName(eventName), eventName).GetAwaiter().GetResult();
                ruleExists = rule != null;
            }
            catch (MessagingEntityNotFoundException)
            {
                ruleExists = false;
            }
            if (!ruleExists)
            {
                subscriptionClient.AddRuleAsync(new RuleDescription
                {
                    Filter = new CorrelationFilter { Label = eventName },
                    Name = eventName
                }).GetAwaiter().GetResult();
            }
        }
        private void RemoveDefaultRule(SubscriptionClient subscriptionClient)
        {
            try
            {
                subscriptionClient
                    .RemoveRuleAsync(RuleDescription.DefaultRuleName)
                    .GetAwaiter().GetResult();
            }
            catch (MessagingEntityNotFoundException)
            {

                _logger.LogWarning("The messaging entity {DefaultRuleName} could not be found.", RuleDescription.DefaultRuleName);
            }
        }
        private SubscriptionClient CreateSubscriptionClient(string eventName)
        {
            return new SubscriptionClient(EventBusConfig.EventBusConnectionString, EventBusConfig.DefaultTopicName, GetSubName(eventName));
        }
        public override void Dispose()
        {
            base.Dispose();
            _topicClient.CloseAsync().GetAwaiter().GetResult();
            _managmentClient.CloseAsync().GetAwaiter ().GetResult();
            _topicClient = null;
            _managmentClient = null;

        }
    }
}
