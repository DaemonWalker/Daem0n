using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc.Internal
{
    class TypeRecord
    {
        public Type Type { get; }
        public ServiceLifetime Lifetime { get; }
        public string ID { get; }
        public TypeRecord(ServiceLifetime lifetime, Type type)
        {
            this.Type = type;
            this.Lifetime = lifetime;
            this.ID = Guid.NewGuid().ToString("N");
        }
    }
}
