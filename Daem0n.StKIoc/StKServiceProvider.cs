using Daem0n.StKIoc.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Daem0n.StKIoc
{
    class StKServiceProvider : IServiceProvider, IServiceScopeFactory, ISupportRequiredService, IDisposable
    {
        private StKServiceCollection serviceCollection;
        internal StKObjectCollection ObjectContainer { get; private set; } = new StKObjectCollection();
        public IServiceScope ServiceScope { get; private set; }
        private bool disposed = false;
        #region 继承接口
        public IServiceScope CreateScope()
        {
            var provider = new StKServiceProvider(this);
            var scope = new StKServiceScope(provider);
            provider.ServiceScope = scope;
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
            this.ServiceScope = new StKServiceScope(this);
        }
        public StKServiceProvider(StKServiceProvider serviceProvider)
        {
            this.serviceCollection = serviceProvider.serviceCollection;
            this.ObjectContainer = serviceProvider.ObjectContainer;
        }
        #endregion
        #region 处理单个对象
        private object GetSingleton(TypeRecord record)
        {
            return ObjectContainer.GetSingleton(this, record);
        }
        private object GetScoped(TypeRecord record)
        {
            return ObjectContainer.GetScoped(this, record);
        }
        private object GetTransient(TypeRecord record)
        {
            return ObjectContainer.GetTransient(this, record);
        }
        #endregion
        #region 处理泛型对象
        private object CreateIEnumerable(Type serviceType)
        {
            var outType = serviceType.GetGenericTypeDefinition();
            var inType = serviceType.GetGenericArguments().First();
            var newType = typeof(List<>).MakeGenericType(inType);
            var list = Activator.CreateInstance(newType) as IList;
            Type checkType = null;
            Type[] parms = null;
            if (serviceCollection.Contains(inType))
            {
                checkType = inType;
            }
            else if (inType.IsGenericType && serviceCollection.Contains(inType.GetGenericTypeDefinition()))
            {
                checkType = inType.GetGenericTypeDefinition();
                parms = inType.GetGenericArguments();
            }

            if (checkType != null)
            {
                var records = serviceCollection.GetAllImplementationTypes(checkType, parms);
                foreach (var record in records)
                {
                    list.Add(this.CallRecord(record));
                }
            }
            return list;
        }
        private object CreateGeneric(Type serviceType)
        {
            var outType = serviceType.GetGenericTypeDefinition();
            var inType = serviceType.GetGenericArguments();
            if (this.serviceCollection.Contains(outType))
            {
                var record = this.serviceCollection.GetImplementationType(outType);
                var newType = record.ImplementationType.MakeGenericType(inType);
                return this.ObjectContainer.GetMakedGeneric(this, record, newType);
            }
            else
            {
                Debug.WriteLine($"Make generictype failed {serviceType.FullName}");
                return null;
            }
        }
        #endregion
        #region 域方法
        internal object Call(Type implementationType, TypeRecord record)
        {
            throw new NotImplementedException();
        }
        internal object CreateInstance(Type implementationType)
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
                parms = constructor.GetParameters().Select(_ => this.GetService(_.ParameterType)).ToArray();
            }
            else
            {
                foreach (var c in constructors)
                {
                    parms = CreateConstructorParams(this, c);
                    if (parms != null)
                    {
                        constructor = c;
                        break;
                    }
                }
            }
            if (constructor.GetParameters().Length == 0)
            {
                parms = new object[0];
            }
            if (parms == null)
            {
                throw new Exception("No fit constuctors");
            }
            var obj = constructor.Invoke(parms);
            return obj;
        }
        private object[] CreateConstructorParams(IServiceProvider serviceProvider, ConstructorInfo constructor)
        {
            var ps = constructor.GetParameters();
            var objs = new List<object>();
            foreach (var p in ps)
            {
                var obj = serviceProvider.GetService(p.ParameterType);
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
        #endregion
        #region 私有方法
        private void Dispose(bool dispoing)
        {
            if (disposed == false)
            {
                this.disposed = true;
                if (dispoing)
                {
                    if (this.IsRootServiceProvider() == false)
                    {
                        this.ObjectContainer.ClearScoped(this.ServiceScope);
                    }
                    else
                    {
                        this.ObjectContainer.Dispose();
                    }
                }
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
                return this.GetTransient(record);
            }
            else
            {
                throw new NotSupportedException($"Unsupported ServiceLifetime {record.Lifetime}");
            }
        }

        ~StKServiceProvider()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
