using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Daem0n.SimIoc.TypeRelataion
{
    public class TypeRelationCollection
    {
        private ImplementitionRelation singleton;
        private ImplementitionRelation tranisent;
        private ImplementitionRelation scoped;
        private IServiceProvider serviceProvider;
        private object GenerateObject(Type type)
        {
            return new object();
        }
        public void Set(Type type, ServiceLifetime lifetime, Func<object> builder)
        {
            Set(type, type, lifetime, builder);
        }
        public void Set(Type type, ServiceLifetime lifetime, object instance)
        {
            Set(type, type, lifetime, instance);
        }
        public void Set(Type tSource, Type tTarget, ServiceLifetime lifetime)
        {
            Set(tSource, tTarget, lifetime, () => GenerateObject(tTarget));
        }
        public void Set(Type tSource, Type tTarget, ServiceLifetime lifetime, object targetInstance)
        {
            Set(tSource, tTarget, lifetime, () => targetInstance);
        }
        public void Set(Type tSource, Type tTarget, ServiceLifetime lifetime, Func<object> builder)
        {
            if (lifetime == ServiceLifetime.Scoped)
            {
                scoped.Add(tSource, tTarget, builder);
            }
            else if (lifetime == ServiceLifetime.Singleton)
            {
                singleton.Add(tSource, tTarget, builder);
            }
            else if (lifetime == ServiceLifetime.Transient)
            {
                tranisent.Add(tSource, tTarget, builder);
            }
        }
        public TypeRelationCollection()
        {
            singleton = new ImplementitionRelation();
            tranisent = new ImplementitionRelation();
            scoped = new ImplementitionRelation();
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
                    //this.RegistObject(descriptor.ServiceType, descriptor.ImplementationInstance, descriptor.Lifetime);
                    //var registration = RegistrationBuilder.ForDelegate(descriptor.ServiceType, (context, parameters) =>
                    //{
                    //    var serviceProvider = context.Resolve<IServiceProvider>();
                    //    return descriptor.ImplementationFactory(serviceProvider);
                    //})
                    //    .ConfigureLifecycle(descriptor.Lifetime, lifetimeScopeTagForSingletons)
                    //    .CreateRegistration();

                    //builder.RegisterComponent(registration);
                    this.Set(descriptor.ServiceType, descriptor.Lifetime, () => descriptor.ImplementationFactory(serviceProvider));
                }
                else
                {
                    this.Set(descriptor.ServiceType, descriptor.Lifetime, descriptor.ImplementationInstance);
                }
            }
        }
    }
}
