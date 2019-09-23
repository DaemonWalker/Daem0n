using Daem0n.SimIoc.Abstractions;
using Daem0n.SimIoc.Intertal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Daem0n.SimIoc.TypeRelataion
{
    public class TypeRelationCollection
    {
        private ImplementationRelation singleton;
        private ImplementationRelation transient;
        private ImplementationRelation scoped;
        private object GenerateObject(Type type)
        {
            return new object();
        }
        internal IRelationContainer Build()
        {
            return new RelationContainer(scoped, singleton, transient);
        }
        public void Set(Type type, ServiceLifetime lifetime, Func<IServiceProvider, object> builder)
        {
            Set(type, type, lifetime, builder, null);
        }
        public void Set(Type type, ServiceLifetime lifetime, object instance)
        {
            Set(type, type, lifetime, instance);
        }
        public void Set(Type tSource, Type tTarget, ServiceLifetime lifetime)
        {
            Set(tSource, tTarget, lifetime, null);
        }
        public void Set(Type tSource, Type tTarget, ServiceLifetime lifetime, object targetInstance)
        {
            Set(tSource, tTarget, lifetime, null, targetInstance);
        }
        public void Set(Type tSource, Type tTarget, ServiceLifetime lifetime, Func<IServiceProvider, object> builder, object instance)
        {
            if (lifetime == ServiceLifetime.Scoped)
            {
                scoped.Add(tSource, tTarget, builder,instance);
            }
            else if (lifetime == ServiceLifetime.Singleton)
            {
                singleton.Add(tSource, tTarget, builder,instance);
            }
            else if (lifetime == ServiceLifetime.Transient)
            {
                transient.Add(tSource, tTarget, builder,instance);
            }
        }
        public TypeRelationCollection()
        {
            singleton = new ImplementationRelation();
            transient = new ImplementationRelation();
            scoped = new ImplementationRelation();
        }

        public void Populate(IServiceCollection serviceDescriptors)
        {
            foreach (var descriptor in serviceDescriptors)
            {
                if (descriptor.ImplementationType != null)
                {
                    Set(descriptor.ServiceType, descriptor.ImplementationType, descriptor.Lifetime);
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    this.Set(descriptor.ServiceType, descriptor.Lifetime, _ => descriptor.ImplementationFactory(_));
                }
                else
                {
                    this.Set(descriptor.ServiceType, descriptor.Lifetime, descriptor.ImplementationInstance);
                }
            }
        }
    }
}
