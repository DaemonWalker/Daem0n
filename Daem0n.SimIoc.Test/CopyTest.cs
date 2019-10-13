using Daem0n.StKIoc;
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
            var factory = new StKServiceProviderFactory();
            return factory.CreateServiceProvider(factory.CreateBuilder(serviceCollection));
        }
        [Fact]
        public void SafelyDisposeNestedProviderReferences()
        {
            // Arrange
            var collection = new TestServiceCollection();
            collection.AddTransient<ClassWithNestedReferencesToProvider>();
            var provider = CreateServiceProvider(collection);

            // Act
            var nester = provider.GetService<ClassWithNestedReferencesToProvider>();

            // Assert
            Assert.NotNull(nester);
            nester.Dispose();
            Assert.Null(nester);
        }
    }
}
