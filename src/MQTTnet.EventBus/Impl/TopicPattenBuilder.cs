using MQTTnet.EventBus.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace MQTTnet.EventBus.Impl
{
    public class TopicPattenBuilder : ITopicPattenBuilder
    {
        public string GetTopicEntity(string topicPattern, string topic, string name)
        {
            int index = 0;
            var topicInfo = Parse(topic).ToArray();
            foreach (var patterntInfo in Parse(topicPattern))
            {
                if (!patterntInfo.IsConst)
                {
                    if (patterntInfo.Name == name)
                        return topicInfo[index].Name;
                }

                index++;
                if (index == topicInfo.Length)
                    break;
            }

            return null;
        }

        public void SetData(object model, string topicPattern, string topic)
        {
            var modelType = model.GetType();

            int index = 0;
            var topicInfo = Parse(topic).ToArray();
            foreach (var patterntInfo in Parse(topicPattern))
            {
                if (!patterntInfo.IsConst)
                {
                    var pi = modelType.GetProperty(patterntInfo.Name);
                    if (pi.SetMethod != null)
                    {
                        var info = topicInfo[index];
                        if (pi.PropertyType == typeof(string))
                        {
                            pi.SetValue(model, info.Name);
                        }
                        else
                        {
                            var value = Convert.ChangeType(info.Name, pi.PropertyType);
                            pi.SetValue(model, value);
                        }
                    }
                }

                index++;
                if (index == topicInfo.Length)
                    return;
            }
        }

        public Expression<Func<object, string>> CreateTopic(Type eventType, string topicPattern)
        {
            var pEvent = Expression.Parameter(typeof(object), "ev");

            var topicInfos = Parse(topicPattern);
            var format = ConvertToFormattedString(topicInfos);

            var formatExp = Expression.Constant(format, typeof(string));

            Type stringType = typeof(string);
            var argExps = topicInfos.Where(p => !p.IsConst).Select(ti =>
            {
                var prop = eventType.GetProperty(ti.Name);
                var propExp = Expression.Property(
                    Expression.Convert(pEvent, eventType), prop);
                if (prop.PropertyType != stringType)
                {
                    return (Expression)Expression.Call(propExp, typeof(object).GetMethod(nameof(object.ToString)));
                }

                return propExp;
            });

            var argExp = Expression.NewArrayInit(typeof(object), argExps);
            var body = Expression.Call(null, ReflectionHelper.GetStringFormatMethod(), formatExp, argExp);

            return Expression.Lambda<Func<object, string>>(body, pEvent);
        }

        public Expression<Func<TEvent, string>> CreateTopic<TEvent>(string topicPattern)
        {
            var eventType = typeof(TEvent);
            var pEvent = Expression.Parameter(eventType, "ev");

            var topicInfos = Parse(topicPattern);
            var format = ConvertToFormattedString(topicInfos);

            var formatExp = Expression.Constant(format, typeof(string));

            Type stringType = typeof(string);
            var argExps = topicInfos.Where(p => !p.IsConst).Select(ti =>
            {
                var prop = eventType.GetProperty(ti.Name);
                var propExp = Expression.Property(pEvent, prop);
                if (prop.PropertyType != stringType)
                {
                    return (Expression)Expression.Call(propExp, typeof(object).GetMethod(nameof(object.ToString)));
                }

                return propExp;
            });

            var argExp = Expression.NewArrayInit(typeof(object), argExps);
            var body = Expression.Call(null, ReflectionHelper.GetStringFormatMethod(), formatExp, argExp);

            return Expression.Lambda<Func<TEvent, string>>(body, pEvent);
        }

        public IEnumerable<TopicPatternInfo> Parse(string topicPattern)
        {
            if (string.IsNullOrEmpty(topicPattern))
                return Enumerable.Empty< TopicPatternInfo>();

            return topicPattern.Trim('/').Split('/').Select(p =>
            {
                if (p.StartsWith("{") && p.EndsWith("}"))
                {
                    return new TopicPatternInfo
                    {
                        IsConst = false,
                        Name = p.Substring(1, p.Length - 2)
                    };
                }

                return new TopicPatternInfo
                {
                    IsConst = true,
                    Name = p
                };
            }).ToArray();
        }

        public string ConvertToFormattedString(IEnumerable<TopicPatternInfo> patternInfos)
        {
            var builder = new StringBuilder("/");
            int index = 0;
            foreach (var info in patternInfos)
            {
                if (info.IsConst)
                    builder.Append(info.Name);
                else
                {
                    builder.Append("{").Append(index).Append("}");
                    index++;
                }

                builder.Append("/");
            }
            return builder.ToString().TrimEnd('/');
        }
    }
}
