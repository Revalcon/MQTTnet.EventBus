namespace MQTTnet.EventBus
{
    public static class StaticCache
    {
        internal static IEventProvider EventProvider = new EventProvider();

        public static void Clear()
        {
            EventProvider = null;
        }
    }
}
