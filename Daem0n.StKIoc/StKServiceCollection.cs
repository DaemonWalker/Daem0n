using Daem0n.StKIoc.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daem0n.StKIoc
{
    public class StKServiceCollection
    {
        private TypeRelationCollection relations;
        private IServiceProvider serviceProvider;
        public StKServiceCollection(IServiceCollection serviceDescriptors)
        {
            relations = new TypeRelationCollection();
            foreach (var service in serviceDescriptors)
            {
                if (service.ImplementationInstance != null)
                {
                    var id = relations.Add(service.ServiceType, service.ServiceType, service.Lifetime, instance: service.ImplementationInstance).ID;
                }
                else if (service.ImplementationFactory != null)
                {
                    var id = relations.Add(service.ServiceType, service.ImplementationType ?? service.ServiceType, service.Lifetime, factory: service.ImplementationFactory).ID;
                }
                else
                {
                    var id = relations.Add(service.ServiceType, service.ImplementationType, service.Lifetime, factory: _ => _.GetService(service.ServiceType)).ID;
                }
            }
        }
        public IServiceProvider Build()
        {
            this.serviceProvider = new StKServiceProvider(this);
            this.relations.Add(typeof(IServiceProvider), typeof(StKServiceProvider), ServiceLifetime.Singleton, instance: this.serviceProvider);
            this.relations.Add(typeof(IServiceScope), typeof(StKServiceScope), ServiceLifetime.Scoped);
            return this.serviceProvider;
        }
        internal TypeRecord GetImplementationType(Type serviceType)
        {
            return relations.Get(serviceType);
        }
        internal TypeRecord GetImplementationType(Type serviceType, ServiceLifetime lifetime)
        {
            var l = relations.GetAll(serviceType).Where(p => p.Lifetime == lifetime);
            return l?.FirstOrDefault();
        }
        internal bool Contains(Type serviceType)
        {
            return this.relations.Contains(serviceType);
        }
        internal List<TypeRecord> GetAllImplementationTypes(Type serviceType)
        {
            var records = this.relations.GetAll(serviceType);
            var list = new List<TypeRecord>();
            foreach (var record in records)
            {
                if (record.ImplementationType.IsConstructedGenericType)
                {
                    var newType = record.ImplementationType.MakeGenericType(serviceType.GetGenericArguments());
                    var newRec = new TypeRecord(ServiceLifetime.Transient, serviceType, newType, null, _ => _.GetService(newType));
                    list.Add(newRec);
                }
                else
                {
                    list.Add(record);
                }
            }
        }
    }
}
