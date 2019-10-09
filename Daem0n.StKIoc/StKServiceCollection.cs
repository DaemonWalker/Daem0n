using Daem0n.StKIoc.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daem0n.StKIoc
{
    class StKServiceCollection
    {

        private TypeRelationCollection relations;
        private Dictionary<string, ObjectContainer> objectContainers;
        private IServiceProvider serviceProvider;
        public StKServiceCollection(IServiceCollection serviceDescriptors)
        {
            relations = new TypeRelationCollection();
            foreach (var service in serviceDescriptors)
            {
                if (service.ImplementationInstance != null)
                {
                    var id = relations.Add(service.ServiceType, service.ServiceType, service.Lifetime).ID;
                    objectContainers.Add(id, new ObjectContainer(service.ImplementationInstance));
                }
                else if (service.ImplementationFactory != null)
                {
                    var id = relations.Add(service.ServiceType, service.ImplementationType ?? service.ServiceType, service.Lifetime).ID;
                    objectContainers.Add(id, new ObjectContainer(service.ImplementationFactory));
                }
                else
                {
                    var id = relations.Add(service.ServiceType, service.ImplementationType, service.Lifetime).ID;
                    objectContainers.Add(id, new ObjectContainer(() => this.serviceProvider.GetService(service.ServiceType)));
                }
            }
        }
        public IServiceProvider Build => this.serviceProvider ?? (serviceProvider = new StKServiceProvider(this));

        public TypeRecord GetImplementationType(Type serviceType)
        {
            return relations.Get(serviceType);
        }
        public TypeRecord GetImplementationType(Type serviceType, ServiceLifetime lifetime)
        {
            var l = relations.GetAll(serviceType).Where(p => p.Lifetime == lifetime);
            return l?.FirstOrDefault();
        }
        public ObjectContainer GetObjectContainer(string id)
        {
            if (objectContainers.ContainsKey(id))
            {
                return null;
            }
            else
            {
                return objectContainers[id];
            }
        }
    }
}
