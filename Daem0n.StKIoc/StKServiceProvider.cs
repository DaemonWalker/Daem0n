using Daem0n.StKIoc.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Daem0n.StKIoc
{
    class StKServiceProvider : IServiceProvider, IServiceScopeFactory, ISupportRequiredService, IDisposable
    {
        private StKServiceCollection serviceCollection;
        private StKObjectCollection objectCollection;
        public IServiceScope ServiceScope { get; private set; }
        private bool disposed = false;
        #region 继承接口
        public IServiceScope CreateScope()
        {
            var provider = new StKServiceProvider(this);
            var scope = new StKServiceScope(provider);
            provider.ServiceScope = ServiceScope;
            return scope;
        }

        public object GetService(Type serviceType)
        {
            var record = this.serviceCollection.GetImplementationType(serviceType);
            if (record != null)
            {
                return CallRecord(record);
            }
            else if (serviceType.IsConstructedGenericType)
            {
                if (serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return CreateIEnumerable(serviceType);
                }
                else
                {
                    return CreateGeneric(serviceType);
                }
            }
            else
            {
                return null;
            }
        }


        public object GetRequiredService(Type serviceType)
        {
            return this.GetService(serviceType);
        }
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        #region 构造函数
        public StKServiceProvider(StKServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }
        public StKServiceProvider(StKServiceProvider serviceProvider)
        {
            this.serviceCollection = serviceProvider.serviceCollection;
            this.objectCollection = serviceProvider.objectCollection;
        }
        #endregion
        #region 处理单个对象
        private object GetSingleton(TypeRecord record)
        {
            return objectCollection.GetSingleton(this, record);
        }
        private object GetScoped(TypeRecord record)
        {
            return objectCollection.GetScoped(this, record);
        }
        private object GetTransient(TypeRecord record)
        {
            return objectCollection.GetTransient(this, record);
        }
        #endregion
        #region 处理泛型对象
        private object CreateIEnumerable(Type serviceType)
        {
            var outType = serviceType.GetGenericTypeDefinition();
            var inType = serviceType.GetGenericArguments().First();
            if (serviceCollection.Contains(serviceType))
            {
                var records = serviceCollection.GetAllImplementationTypes(serviceType);

            }
            else
            {
                var newType = typeof(List<>).MakeGenericType(inType);
                return Activator.CreateInstance(newType);
            }
        }
        private object CreateGeneric(Type serviceType)
        {

        }
        #endregion
        #region 域方法
        internal object Call(Type implementationType)
        {
            var constructors = implementationType.GetConstructors().Where(p => p.IsPublic).OrderByDescending(p => p.GetParameters().Length).ToArray();
            ConstructorInfo constructor = null;
            object[] parms = null;
            if (constructors.Length == 0)
            {
                throw new NotSupportedException($"There no public constructor for {implementationType.FullName}");
            }
            if (constructors.Length == 1)
            {
                constructor = constructors.First();

            }
            else
            {
                foreach (var c in constructors)
                {
                    parms = CreateConstructorParams(c);
                    if (parms != null)
                    {
                        break;
                    }
                }
            }
        }
        #endregion
        #region 私有方法
        private void Dispose(bool dispoing)
        {
            if (disposed == false)
            {
                if (dispoing)
                {
                    if (this.IsRootServiceProvider() == false)
                    {
                        this.objectCollection.ClearScoped(this.ServiceScope);
                    }
                    else
                    {
                        this.objectCollection.Dispose();
                    }
                }
                this.disposed = true;
            }
        }

        private object CallRecord(TypeRecord record)
        {
            if (record.Lifetime == ServiceLifetime.Singleton)
            {
                return this.GetSingleton(record);
            }
            else if (record.Lifetime == ServiceLifetime.Scoped)
            {
                return this.GetScoped(record);
            }
            else if (record.Lifetime == ServiceLifetime.Transient)
            {
                return this.GetSingleton(record);
            }
            else
            {
                throw new NotSupportedException($"Unsupported ServiceLifetime {record.Lifetime}");
            }
        }
        private object[] CreateConstructorParams(ConstructorInfo constructor)
        {
            var ps = constructor.GetParameters();
            var objs = new List<object>();
            foreach (var p in ps)
            {
                var obj = this.GetService(p.ParameterType);
                if (obj == null)
                {
                    return null;
                }
                else
                {
                    objs.Add(obj);
                }
            }
            return objs.ToArray();
        }
        ~StKServiceProvider()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
