using Microsoft.Extensions.DependencyInjection;
using System;

namespace MQTTnet.EventBus.DependencyInjection.Builder
{
    public interface IServicesBuilder
    {
        IServicesBuilder AddServices(Action<IServiceCollection> addServices, ServiceType serviceType = ServiceType.Custom);
    }
}
