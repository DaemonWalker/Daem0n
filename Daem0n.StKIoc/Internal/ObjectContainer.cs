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
        public ObjectContainer(object instance, ServiceLifetime lifetime) : this(instance, lifetime, serviceScope: null) { }
        public ObjectContainer(object instance, ServiceLifetime lifetime, string id) : this(instance, lifetime, null, id) { }
        public ObjectContainer(object instance, ServiceLifetime lifetime, IServiceScope serviceScope)
            : this(instance, lifetime, serviceScope, ToolUtils.GenerateID()) { }
        public ObjectContainer(object instance, ServiceLifetime lifetime, IServiceScope serviceScope, string id)
        {
            this.Instance = instance;
            this.Lifetime = lifetime;
            this.ServiceScope = serviceScope;
            this.ID = id;
        }
        public override string ToString()
        {
            return this.Instance.ToString();
        }
    }
}
