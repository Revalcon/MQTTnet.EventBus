using System;
using System.Collections.Generic;

namespace MQTTnet.EventBus.DependencyInjection.Builder.Impl
{
    public class EventOptionsBuilder : IEventOptionsBuilder, IBuilder<HashSet<EventOptions>>
    {
        private readonly HashSet<EventOptions> _consumerOptions;

        public EventOptionsBuilder()
        {
            _consumerOptions = new HashSet<EventOptions>(ComparersManager.EventOptions);
        }

        public IEventOptionsBuilder AddConsumer<TEvent, TConsumer>(string eventName, Action<IMessageBuilder<TEvent>> convertorConfigurator)
            where TConsumer : IConsumer<TEvent>
        {
            var options = new EventOptions(eventName)
            {
                ConsumerType = typeof(TConsumer),
                EventType = typeof(TEvent)
            };

            var builder = new MessageBuilder<TEvent>(options);
            convertorConfigurator.Invoke(builder);

            _consumerOptions.Add(options);
            return this;
        }

        public HashSet<EventOptions> Build() => _consumerOptions;
    }
}
