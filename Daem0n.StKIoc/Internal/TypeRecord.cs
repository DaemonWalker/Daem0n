using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc.Internal
{
    class TypeRecord
    {
        public Type ServiceType { get; }
        public Type ImplementationType { get; }
        public bool BuildFlag { get; }
        public ServiceLifetime Lifetime { get; }
        public string ID { get; }
        private object instance;
        private Func<IServiceProvider, object> factory;
        public TypeRecord(ServiceLifetime lifetime, Type service, Type implementation, object instance, Func<IServiceProvider, object> factory, bool buildFlag = true, string id = null)
        {
            this.ServiceType = service;
            this.ImplementationType = implementation;
            this.Lifetime = lifetime;
            this.ID = id ?? ToolUtils.GenerateID();
            this.instance = instance;
            this.factory = factory;
            this.BuildFlag = buildFlag;
        }

        public object GetObject(IServiceProvider serviceProvider)
        {
            if (this.instance != null)
            {
                return this.instance;
            }
            if (this.factory != null)
            {
                return factory.Invoke(serviceProvider);
            }
            return null;
        }
        public object GetInstacne(IServiceProvider serviceProvider)
        {
            if (this.instance == null)
            {
                if (this.factory != null)
                {
                    this.instance = this.factory.Invoke(serviceProvider);
                }
                else
                {
                    return null;
                }
            }
            return this.instance;
        }
        public object CallFactory(IServiceProvider serviceProvider)
        {
            return this.factory?.Invoke(serviceProvider);
        }
        public override string ToString() => $"{this.ServiceType} -> {this.ImplementationType}";

    }
}
