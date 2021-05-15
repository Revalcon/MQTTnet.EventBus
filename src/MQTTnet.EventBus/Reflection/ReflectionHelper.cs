using System.Linq;
using System.Reflection;

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
    }
}
