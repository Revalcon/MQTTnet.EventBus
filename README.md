# MQTTnet.EventBus
[![GitHub](https://img.shields.io/github/license/arttonoyan/MQTTnet.EventBus.svg)](https://github.com/arttonoyan/MQTTnet.EventBus/blob/master/LICENSE)
[![Nuget](https://img.shields.io/nuget/v/MQTTnet.EventBus.svg)](https://www.nuget.org/packages/MQTTnet.EventBus/)
[![Nuget](https://img.shields.io/nuget/dt/MQTTnet.EventBus.svg)](https://www.nuget.org/packages/MQTTnet.EventBus/)

## Quickstart
In your ASP.NET Core Startup.cs file add the following
``` csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

    var retryCount = 5;
    services.AddMqttEventBus(cfg =>
    {
        cfg
            .WithClientId("Api")
            .WithTcpServer("{Ip Address}", port: 1883);

    }, retryCount);
    services.AddTransient<IntegrationEventHandler>();
}
```
