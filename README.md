# MQTTnet.EventBus
[![GitHub](https://img.shields.io/github/license/arttonoyan/MQTTnet.EventBus.svg)](https://github.com/arttonoyan/MQTTnet.EventBus/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/MQTTnet.EventBus.svg)](https://www.nuget.org/packages/MQTTnet.EventBus/)
[![Nuget](https://img.shields.io/nuget/dt/MQTTnet.EventBus.svg)](https://www.nuget.org/packages/MQTTnet.EventBus/)

## Quick Start
In your ASP.NET Core Startup.cs file add the following
``` csharp
public void ConfigureServices(IServiceCollection services)
{
    //...

    var retryCount = 5;
    services.AddMqttEventBus(cfg =>
    {
        cfg
            .WithClientId("Api")
            .WithTcpServer("{Ip Address}", port: 1883);

    }, retryCount);
    services.AddTransient<MyEventHandler>();
}
```
An EventHandler is a class that may handle one or more message types. Each message type is defined by the IIntegrationEventHandler<in T> interface, where T is the MqttApplicationMessageReceivedEventArgs.
    
```csharp
public class MyEventHandler : IIntegrationEventHandler
{
    public Task Handle(MqttApplicationMessageReceivedEventArgs args)
    {
        //Some action...
        return Task.CompletedTask;
    }
}
```
Then in your application add this extension
```csharp
public static class ApplicationBuilderExtansions
{
    public static IApplicationBuilder UseEventBus(this IApplicationBuilder app, Action<IEventBus> action)
    {
        var eventBus = app.ApplicationServices.GetRequiredService<IEventBus>();
        action.Invoke(eventBus);
        return app;
    }
}
```
and use it in your Startup.cs file
```csharp
public void Configure(IApplicationBuilder app, IHostingEnvironment env)
{
    //...

    app.UseEventBus(async bus => 
    {
        await bus.SubscribeAsync<IntegrationEventHandler>("MyTopic1");
    });
}
```
### Injected interfaces
* IEventBus
* IMqttPersisterConnection
* IEventBusSubscriptionsManager
