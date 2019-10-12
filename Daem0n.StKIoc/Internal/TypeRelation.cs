using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daem0n.StKIoc.Internal
{
    class TypeRelationCollection : IDisposable
    {
        private bool disposed = false;
        private HashSet<Type> types = new HashSet<Type>();
        private ConcurrentBag<TypeRecord> bag = new ConcurrentBag<TypeRecord>();
        public TypeRecord Add(Type service, Type implement, ServiceLifetime lifetime, object instance = null, Func<IServiceProvider, object> factory = null, bool buildFlag = true)
        {
            if (types.Contains(service) == false)
            {
                types.Add(service);
            }
            var record = new TypeRecord(lifetime, service, implement, instance, factory, buildFlag);
            bag.Add(record);
            return record;
        }
        public TypeRecord Get(Type service)
        {
            if (types.Contains(service) == false)
            {
                return null;
            }
            return bag.FirstOrDefault(p => p.ServiceType == service);
        }
        public List<TypeRecord> GetAll(Type service)
        {
            if (types.Contains(service) == false)
            {
                return null;
            }
            return bag.Where(_ =>
            {
                if (_.ServiceType == service)
                {
                    return true;
                }
                if (service.IsConstructedGenericType)
                {
                    var outType = service.GetGenericTypeDefinition();
                    if (_.ServiceType == outType)
                    {
                        return true;
                    }
                }
                return false;
            }).ToList();
        }
        public bool Contains(Type service)
        {
            return types.Contains(service);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed == false)
            {
                if (disposing)
                {
                    types = null;
                    bag = null;
                }
                this.disposed = true;
            }
        }
        ~TypeRelationCollection()
        {
            this.Dispose(false);
        }
    }
}
