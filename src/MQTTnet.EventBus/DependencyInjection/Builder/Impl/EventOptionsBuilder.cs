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

        public IEventOptionsBuilder AddEventMapping<TEvent>(string eventName, Action<IEventMappingBuilder<TEvent>> mappingConfigurator)
        {
            var options = new EventOptions(eventName)
            {
                EventType = typeof(TEvent)
            };

            var mappingBuilder = new EventMappingBuilder<TEvent>(options);
            mappingConfigurator.Invoke(mappingBuilder);

            _consumerOptions.Add(options);

            return this;
        }

        public HashSet<EventOptions> Build() => _consumerOptions;
    }
}
