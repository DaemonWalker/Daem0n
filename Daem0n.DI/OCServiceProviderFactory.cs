using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.DI
{
    public class OCServiceProviderFactory : IServiceProviderFactory<ObjectContainer>
    {
        public ObjectContainer CreateBuilder(IServiceCollection services)
        {
            return new ObjectContainer(services);
        }

        public IServiceProvider CreateServiceProvider(ObjectContainer containerBuilder)
        {
            return containerBuilder;
        }
    }
}
