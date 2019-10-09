using Microsoft.Extensions.DependencyInjection;
using System;

namespace Daem0n.StKIoc
{
    public class StKServiceProviderFactory : IServiceProviderFactory<StKServiceCollection>
    {
        public StKServiceCollection CreateBuilder(IServiceCollection services)
        {
            return new StKServiceCollection(services);
        }

        public IServiceProvider CreateServiceProvider(StKServiceCollection containerBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
