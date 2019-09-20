using Daem0n.SimIoc.Abstractions;
using Daem0n.SimIoc.TypeRelataion;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Daem0n.SimIoc
{
    public class ServiceProvider : IServiceProvider, ISupportRequiredService
    {
        private ImplementationRelation singleton;
        private ImplementationRelation transient;
        private ImplementationRelation scoped;
        private ConcurrentDictionary<Type, object> singletonObjects;
        private ConcurrentDictionary<Tuple<Thread, Type>, object> scopedObjects;
        private ConcurrentDictionary<Type, Func<object>> transientObjects;
        internal ServiceProvider(IRelationContainer relationContainer)
        {
            this.singleton = relationContainer.GetSingleton();
            this.transient = relationContainer.GetTransient(); ;
            this.scoped = relationContainer.GetScoped();
        }
        public object GetRequiredService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public object Get(Type type)
        {
            if (singleton.Contains(type))
            {
                var target = singleton.GetTarget(type);
            }
            else if (scoped.Contains(type))
            {

            }
            else if (transient.Contains(type))
            {

            }
            else if (type.IsConstructedGenericType)
            {

            }
            else
            {
                return null;
            }
        }
        private object GetSingleton(Type tSource)
        {
            var tTarget = singleton.GetTarget(tSource);
            if (singletonObjects.TryGetValue(tTarget, out var obj) == false)
            {
                obj = this.CreateObject(singleton.GetBuilder(tTarget));
                if (singletonObjects.TryAdd(tTarget, obj) == false)
                {
                    singletonObjects.TryGetValue(tTarget, out obj);
                }
            }
            return obj;
        }
        private object GetTransient(Type tSource)
        {
            var tTarget = transient.GetTarget(tSource);
            return this.CreateObject(transient.GetBuilder(tTarget));
        }
        private object GetScoped(Type tSource)
        {
            var tTarget = scoped.GetTarget(tSource);
            var key = new Tuple<Thread, Type>(Thread.CurrentThread, tSource);
            if (scopedObjects.TryGetValue(key, out var obj))
            {
                obj = this.CreateObject(scoped.GetBuilder(tTarget));
                if (scopedObjects.TryAdd(key, obj) == false)
                {
                    scopedObjects.TryGetValue(key, out obj);
                }
            }
            return obj;
        }

        private object CreateObject(Type tSource, Type tTarget, BuilderInfo builder)
        {
            if (builder.Instance != null)
            {
                return builder.Instance;
            }
            else if (builder.Builder != null)
            {
                return builder.Builder(this);
            }
            else
            {
                return CreateObject(tTarget);
            }
        }
        private object CreateObject(Type t)
        {
            var constructors = t.GetConstructors().Where(p => p.IsPublic == true).ToList();
            ConstructorInfo constructor = null;
            ParameterInfo[] parms = null;
            if (constructors.Count == 0)
            {
                throw new Exception("No Constructors");
            }
            if (constructors.Count == 1)
            {
                constructor = constructors.First();
                parms = constructor.GetParameters();
            }
            else
            {

            }
        }
        private bool CheckType(Type tSource)
        {
            if (singleton.Contains(tSource))
            {
                return true;
            }
            else if (scoped.Contains(tSource))
            {
                return true;
            }
            else if (transient.Contains(tSource))
            {
                return true;
            }
            else if (tSource.IsConstructedGenericType)
            {
                var outType = tSource.GetGenericTypeDefinition();
                if (outType == typeof(IEnumerable<>))
                {
                    return CheckType(tSource.GenericTypeArguments.First());
                }
                else
                {
                    return CheckType(outType);
                }
            }
        }
    }
}
