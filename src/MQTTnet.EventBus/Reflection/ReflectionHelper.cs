using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MQTTnet.EventBus.Reflection
{
    public static class ReflectionHelper
    {
        public static MethodInfo GetStringFormatMethod()
        {
            return typeof(string)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(p =>
                {
                    if (p.Name == nameof(string.Format))
                    {
                        var parametrs = p.GetParameters();
                        if (parametrs.Length == 2)
                        {
                            if (parametrs[0].ParameterType == typeof(string) && parametrs[1].ParameterType == typeof(object[]))
                                return true;
                        }
                    }

                    return false;
                })
                .FirstOrDefault();
        }

        public static string CreateTopicPattern<T>(Expression<Func<T, string>> patternExp)
        {
            var constBuilder = new StringBuilder();
            var members = new List<string>();
            new TopicPatternVisitor(constBuilder, members).Visit(patternExp);

            var format = constBuilder.ToString();
            string pattern = members.Count > 0 ? string.Format(format, members.ToArray()) : format;
            return pattern;
        }
    }
}
