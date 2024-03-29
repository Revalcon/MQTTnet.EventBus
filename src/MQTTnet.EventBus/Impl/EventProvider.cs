﻿using MQTTnet.EventBus.Exceptions;
using MQTTnet.EventBus.Logger;
using MQTTnet.EventBus.Serializers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MQTTnet.EventBus.Impl
{
    public class EventProvider : IEventProvider
    {
        private readonly IDictionary<string, EventCreater> _eventCreaters;
        private readonly IDictionary<string, EventOptions> _eventOptions;
        private readonly IDictionary<Type, string> _eventNames;
        private readonly IDictionary<string, Func<object, string>> _topicCreaters;
        private readonly IDictionary<string, string> _topics;
        private readonly ITopicPattenBuilder _topicPattenBuilder;
        private readonly IEventBusLogger<EventProvider> _logger;

        public EventProvider() : this(null, null, new HashSet<EventOptions>(), null) { }

        public EventProvider(IServiceProvider serviceProvider, ITopicPattenBuilder topicPattenBuilder, HashSet<EventOptions> eventOptions, IEventBusLogger<EventProvider> logger)
        {
            _topicPattenBuilder = topicPattenBuilder;
            _eventCreaters = eventOptions.ToDictionary(p => p.EventName, p => EventCreater.New(serviceProvider, p));
            _eventOptions = eventOptions.ToDictionary(p => p.EventName);
            _eventNames = eventOptions.ToDictionary(p => p.EventType, p => p.EventName);

            _topicCreaters = eventOptions
                    .Where(p => p.Topic.PatternType != null && _topicPattenBuilder.IsPattern(p.Topic.Pattern))
                    .ToDictionary(p => p.EventName, p => topicPattenBuilder.CreateTopicCreater(p.Topic.PatternType, p.Topic.Pattern).Compile());

            _topics = eventOptions
                .Where(p => !_topicPattenBuilder.IsStaticTopic(p.Topic.Pattern))
                .ToDictionary(p => p.EventName, p => p.Topic.Pattern);

            _logger = logger;
        }

        public MqttApplicationMessage CreateMessage(string eventName, object @event, string topic)
        {
            if (_eventCreaters.TryGetValue(eventName, out var eventCreater))
                return eventCreater.CreateMqttApplicationMessage(@event, topic);

            Type eventType = null;
            if (TryGetEventOptions(eventName, out var options))
                eventType = options.EventType;

            throw new EventNotFoundException(eventName, eventType);
        }

        public string GetTopic(string eventName)
        {
            if (_topics.TryGetValue(eventName, out string topic))
                return topic;
            return string.Empty;
        }

        public string GetTopic(string eventName, object topicInfo)
        {
            if (_topicCreaters.TryGetValue(eventName, out var topicCreater))
                return topicCreater.Invoke(topicInfo);
            return string.Empty;
        }

        public bool HasTopicPattern(string eventName)
        {
            if (_eventOptions.TryGetValue(eventName, out var opions))
                return !string.IsNullOrEmpty(opions.Topic.Pattern);
            return false;
        }

        public bool TryGetEventName(Type eventType, out string eventName)
            => _eventNames.TryGetValue(eventType, out eventName);

        public string GetTopicEntity(string eventName, string topic, string name)
        {
            if (_eventOptions.TryGetValue(eventName, out var opions))
            {
                if (!opions.Topic.HasPattern)
                    return string.Empty;

                return _topicPattenBuilder.GetTopicEntity(opions.Topic.Pattern, topic, name);
            }

            return string.Empty;
        }

        public bool TrySetTopicInfo(string eventName, object @event, string topic)
        {
            try
            {
                if (_eventOptions.TryGetValue(eventName, out var opions))
                {
                    if (!opions.Topic.HasPattern)
                        return false;

                    _topicPattenBuilder.SetData(@event, opions.Topic.Pattern, topic);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"eventName: {eventName}, EvenType: {@event.GetType()}, Topic: {topic}");
            }

            return false;
        }

        public bool TryGetEventOptions(string eventName, out EventOptions options) =>
            _eventOptions.TryGetValue(eventName, out options);

        public IEnumerator<EventOptions> GetEnumerator() =>
            _eventOptions.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private class EventCreater
        {
            public EventCreater(IServiceProvider serviceProvider, EventOptions eventOptions)
            {
                _serializerMethod = eventOptions.ConverterType.GetMethod(nameof(IEventSerializer<object>.Serialize));
                _converter = new Lazy<object>(() => serviceProvider.GetService(eventOptions.ConverterType));
                MessageCreater = eventOptions.MessageCreater;
            }

            private readonly MethodInfo _serializerMethod;
            
            private readonly Lazy<object> _converter;
            public object Converter => _converter.Value;
            public Action<MqttApplicationMessageBuilder> MessageCreater { get; }

            public byte[] Serialize(object @event)
                => (byte[])_serializerMethod.Invoke(Converter, new object[] { @event });

            public MqttApplicationMessage CreateMqttApplicationMessage(object @event, string topic)
            {
                var data = Serialize(@event);

                var builder = new MqttApplicationMessageBuilder();
                builder.WithTopic(topic);
                builder.WithPayload(data);
                MessageCreater.Invoke(builder);

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
