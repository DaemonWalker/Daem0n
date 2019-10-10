using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc
{
    static class ExtenssionMethods
    {
        public static bool IsRootServiceProvider(this IServiceProvider serviceProvider)
        {
            return serviceProvider.GetService(typeof(IServiceProvider)) == serviceProvider;
        }
        public static IServiceScope GetScope(this IServiceProvider serviceProvider)
        {
            if (serviceProvider is StKServiceProvider provider)
            {
                return provider.ServiceScope;
            }
            else
            {
                throw new Exception("ServiceProvider Type Error");
            }
        }
        public static object CallObject(this IServiceProvider serviceProvider)
        {
            if (serviceProvider is StKServiceProvider provider)
            {

            }
        }
    }
}
