using System;

namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    public interface IEventMappingBuilder<TEvent>
    {
        IEventMappingBuilder<TEvent> AddConsumer<TConsumer>() where TConsumer : IConsumer<TEvent>;
        IEventMappingBuilder<TEvent> UseConverter<TConverter>() where TConverter : Serializers.IEventConverter<TEvent>;
        IEventMappingBuilder<TEvent> UseTopicPattern(string value);
        IEventMappingBuilder<TEvent> UseMessageBuilder(Action<MqttApplicationMessageBuilder> messageBuilderConfigurator);
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    using MQTTnet.EventBus.DependencyInjection.Builder;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Text;

    public static class EventMappingBuilderExtensions
    {
        public static IEventMappingBuilder<TEvent> UseTopicPattern<TEvent>(this IEventMappingBuilder<TEvent> builder, Expression<Func<TEvent, string>> patternExp)
        {
            var constBuilder = new StringBuilder();
            var members = new List<string>();
            new TopicPatternVisitor(constBuilder, members).Visit(patternExp);

            var format = constBuilder.ToString();
            string pattern = members.Count > 0 ? string.Format(format, members.ToArray()) : format;

            return builder.UseTopicPattern(pattern);
        }

        private class TopicPatternVisitor : ExpressionVisitor
        {
            private readonly StringBuilder _constBuilder;
            private readonly List<string> _members;

            public TopicPatternVisitor(StringBuilder builder, List<string> members)
            {
                _constBuilder = builder;
                _members = members;
            }

            public override Expression Visit(Expression node)
            {
                if (node != null)
                {
                    if (node.NodeType == ExpressionType.Constant)
                    {
                        _constBuilder.Append(((ConstantExpression)node).Value);
                    }
                    else if (node.NodeType == ExpressionType.MemberAccess)
                    {
                        _members.Add("{" + ((MemberExpression)node).Member.Name + "}");
                    }
                }

                return base.Visit(node);
            }
        }
    }
}
