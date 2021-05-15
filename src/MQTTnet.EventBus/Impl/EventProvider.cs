using MQTTnet.EventBus.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MQTTnet.EventBus.Impl
{
    public class EventProvider : IEventProvider
    {
        private readonly IDictionary<string, EventCreater> _eventCreaters;
        private readonly IDictionary<string, EventOptions> _eventOptions;
        private readonly IDictionary<string, Func<object, string>> _topicCreaters;
        private readonly ITopicPattenBuilder _topicPattenBuilder;

        public EventProvider() : this(null, null, new HashSet<EventOptions>()) { }

        public EventProvider(IServiceProvider serviceProvider, ITopicPattenBuilder topicPattenBuilder, HashSet<EventOptions> eventOptions)
        {
            _topicPattenBuilder = topicPattenBuilder;
            _eventCreaters = eventOptions.ToDictionary(p => p.EventName, p => EventCreater.New(serviceProvider, p));
            _eventOptions = eventOptions.ToDictionary(p => p.EventName);
            _topicCreaters = eventOptions.ToDictionary(p => p.EventName, p => topicPattenBuilder.CreateTopic(p.EventType, p.TopicPattern).Compile());
        }

        public MqttApplicationMessage CreateMessage(string eventName, object @event, string topic)
        {
            return _eventCreaters[eventName].CreateMqttApplicationMessage(@event, topic);
        }

        public string GetTopic(string eventName, object @event)
        {
            if (_topicCreaters.TryGetValue(eventName, out var topicCreater))
                return topicCreater.Invoke(@event);
            return string.Empty;
        }

        public bool SetTopicInfo(string eventName, object @event, string topic)
        {
            try
            {
                if (_eventOptions.TryGetValue(eventName, out var opions))
                {
                    _topicPattenBuilder.SetData(@event, opions.TopicPattern, topic);
                    return true;
                }
            }
            catch (Exception ex)
            {
                //TODO [Logger] [Artyom Tonoyan] [15/05/2021]: Add log
            }
            
            return false;
        }

        public Type GetConsumerType(string eventName)
        {
            if (_eventOptions.TryGetValue(eventName, out var options))
                return options.ConsumerType;
            return null;
        }

        public Type GetConverterType(string eventName)
            => _eventOptions[eventName].ConverterType;

        private class EventCreater
        {
            public EventCreater(IServiceProvider serviceProvider, EventOptions eventOptions)
            {
                _serializerMethod = eventOptions.ConverterType.GetMethod(nameof(IEventSerializer<object>.Serialize));
                Converter = serviceProvider.GetService(eventOptions.ConverterType);
                MessageCreater = eventOptions.MessageCreater;
            }

            private readonly MethodInfo _serializerMethod;
            public object Converter { get; }
            public Action<MqttApplicationMessageBuilder> MessageCreater { get; }

            public byte[] Serialize(object @event)
                => (byte[])_serializerMethod.Invoke(Converter, new object[] { @event });

            public MqttApplicationMessage CreateMqttApplicationMessage(object @event, string topic)
            {
                var data = Serialize(@event);

                DateTime date = DateTime.Now;
                //date.date

                var builder = new MqttApplicationMessageBuilder();
                MessageCreater.Invoke(builder);
                builder.WithTopic(topic);
                builder.WithPayload(data);

                return builder.Build();
            }

            public static EventCreater New(IServiceProvider serviceProvider, EventOptions eventOptions)
            {
                if (eventOptions.MessageCreater is null)
                    eventOptions.MessageCreater = builder => builder.WithRetainFlag();

                return new EventCreater(serviceProvider, eventOptions);
            }
        }
    }
}
