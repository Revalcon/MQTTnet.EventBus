using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace MQTTnet.EventBus.DependencyInjection.Builder.Impl
{
    public class ServicesBuilder : IServicesBuilder
    {
        private readonly IServiceCollection _services;
        private readonly HashSet<ServiceType> _addedServices;

        public ServicesBuilder(IServiceCollection services, HashSet<ServiceType> tempreryData)
        {
            _services = services;
            _addedServices = tempreryData;
        }

        public IServicesBuilder AddServices(Action<IServiceCollection> addServices, ServiceType serviceType = ServiceType.Custom)
        {
            _addedServices.Add(serviceType);
            addServices.Invoke(_services);
            return this;
        }
    }
}
