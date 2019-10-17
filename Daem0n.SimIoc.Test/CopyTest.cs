using Daem0n.StKIoc;
using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
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
        public class ClassWithNestedReferencesToProvider : IDisposable
        {
            private IServiceProvider _serviceProvider;
            private ClassWithNestedReferencesToProvider _nested;

            public ClassWithNestedReferencesToProvider(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
                _nested = new ClassWithNestedReferencesToProvider(_serviceProvider, 0);
            }

            private ClassWithNestedReferencesToProvider(IServiceProvider serviceProvider, int level)
            {
                _serviceProvider = serviceProvider;
                if (level > 1)
                {
                    _nested = new ClassWithNestedReferencesToProvider(_serviceProvider, level + 1);
                }
            }

            public void Dispose()
            {
                _nested?.Dispose();
                (_serviceProvider as IDisposable)?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}
