using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Xunit;

namespace Daem0n.SimIoc.Test
{
    public class CopyTest
    {
        protected IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var factory = new ServiceProviderFactory();
            return factory.CreateServiceProvider(factory.CreateBuilder(serviceCollection));
        }
        [Theory]
        [InlineData(typeof(IFakeService), typeof(FakeService), typeof(IFakeService), ServiceLifetime.Scoped)]
        [InlineData(typeof(IFakeService), typeof(FakeService), typeof(IFakeService), ServiceLifetime.Singleton)]
        [InlineData(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), typeof(IFakeOpenGenericService<IServiceProvider>), ServiceLifetime.Scoped)]
        [InlineData(typeof(IFakeOpenGenericService<>), typeof(FakeOpenGenericService<>), typeof(IFakeOpenGenericService<IServiceProvider>), ServiceLifetime.Singleton)]
        public void ResolvesDifferentInstancesForServiceWhenResolvingEnumerable(Type serviceType, Type implementation, Type resolve, ServiceLifetime lifetime)
        {
            // Arrange
            var serviceCollection = new TestServiceCollection
            {
                ServiceDescriptor.Describe(serviceType, implementation, lifetime),
                ServiceDescriptor.Describe(serviceType, implementation, lifetime),
                ServiceDescriptor.Describe(serviceType, implementation, lifetime)
            };

            var serviceProvider = CreateServiceProvider(serviceCollection);
            using (var scope = serviceProvider.CreateScope())
            {
                var enumerable = (scope.ServiceProvider.GetService(typeof(IEnumerable<>).MakeGenericType(resolve)) as IEnumerable)
                    .OfType<object>().ToArray();
                var service = scope.ServiceProvider.GetService(resolve);

                // Assert
                Assert.Equal(3, enumerable.Length);
                Assert.NotNull(enumerable[0]);
                Assert.NotNull(enumerable[1]);
                Assert.NotNull(enumerable[2]);

                Assert.NotEqual(enumerable[0], enumerable[1]);
                Assert.NotEqual(enumerable[1], enumerable[2]);
                Assert.Equal(service, enumerable[2]);
            }
        }
    }
}
