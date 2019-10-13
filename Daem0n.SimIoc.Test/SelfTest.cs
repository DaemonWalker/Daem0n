using Daem0n.StKIoc;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Daem0n.SimIoc.Test
{
    public class SelfTest
    {
        private IServiceProvider CreateServiceProvider(Func<TestServiceCollection, TestServiceCollection> func)
        {
            var collection = new TestServiceCollection();
            var factory = new StKServiceProviderFactory();
            return factory.CreateServiceProvider(factory.CreateBuilder(func(collection)));
        }
        [Fact]
        public void ScopedTest()
        {
            var serviceProvider = CreateServiceProvider(_ =>
            {
                _.AddScoped(typeof(ISource), typeof(TargetClass));
                return _;
            });
            var obj = serviceProvider.GetService(typeof(ISource));
            Assert.NotNull(obj);
        }
    }
}
