using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc.Internal
{
    class ObjectContainer
    {
        public object Instance { get; private set; }
        public string ID { get; }
        public IServiceScope ServiceScope { get; }
        public ServiceLifetime Lifetime { get; }
        public ObjectContainer(object instance, ServiceLifetime lifetime) : this(instance, lifetime, null) { }
        public ObjectContainer(object instance, ServiceLifetime lifetime, IServiceScope serviceScope)
        {
            this.Instance = instance;
            this.Lifetime = lifetime;
            this.ServiceScope = serviceScope;
            this.ID = ToolUtils.GenerateID();
        }
    }
}
