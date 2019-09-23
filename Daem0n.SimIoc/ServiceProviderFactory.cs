using Daem0n.SimIoc.TypeRelataion;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.SimIoc
{
    public class ServiceProviderFactory : IServiceProviderFactory<TypeRelationCollection>
    {
        public TypeRelationCollection CreateBuilder(IServiceCollection services)
        {
            var factory = new TypeRelationCollection();
            factory.Populate(services);
            return factory;
        }

        public IServiceProvider CreateServiceProvider(TypeRelationCollection containerBuilder)
        {
            return new ServiceProvider(containerBuilder.Build());
        }
    }
}
