using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.SimIoc.Test
{
    class TestServiceCollection : List<ServiceDescriptor>, IServiceCollection
    {
        public TestServiceCollection AddScoped(Type tSource, Type tTarget)
        {
            this.Add(new ServiceDescriptor(tSource, tTarget, ServiceLifetime.Scoped));
            return this;
        }
    }
}
