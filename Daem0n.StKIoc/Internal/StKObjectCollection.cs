using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc.Internal
{
    class StKObjectCollection : IDisposable
    {
        private bool disposed = false;
        private HashSet<string> singleton;
        private HashSet<string> scopedKeys;
        private ConcurrentDictionary<string, object> scoped;
        private ConcurrentBag<object> transient;
        private List<ObjectContainer> objects;
        public object GetSingleton(IServiceProvider serviceProvider, TypeRecord record)
        {
            var obj = record.GetInstacne(serviceProvider);
            if (singleton.Contains(record.ID) == false)
            {
                objects.Add(new ObjectContainer(obj, ServiceLifetime.Singleton));
            }
            return obj;
        }
        public object GetScoped(IServiceProvider serviceProvider, TypeRecord record)
        {
            var key = serviceProvider.GetScope().GetHashCode() + record.ID;
            if (scopedKeys.Contains(key) == false)
            {
                var obj = record.GetObject(serviceProvider);
                scopedKeys.Add(key);
                if (scoped.TryAdd(key, obj))
                {
                    return obj;
                }
                else
                {
                    scoped.TryGetValue(key, out obj);
                    return obj;
                }
            }
            else
            {
                if (scoped.TryGetValue(key, out var obj))
                {
                    return obj;
                }
                else
                {
                    return null;
                }
            }
        }
        public object GetTransient(IServiceProvider serviceProvider, TypeRecord record)
        {
            var obj = record.CallFactory(serviceProvider);
            this.objects.Add(new ObjectContainer(obj, ServiceLifetime.Transient, serviceProvider.GetScope()));
            return obj;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void ClearScoped(IServiceScope serviceScope)
        {
            if (serviceScope == null)
            {
                return;
            }
            var clearList = new List<ObjectContainer>();
            foreach (var oc in this.objects)
            {
                if (oc.ServiceScope == serviceScope)
                {
                    (oc.Instance as IDisposable)?.Dispose();
                    clearList.Add(oc);
                }
            }
            foreach (var oc in clearList)
            {
                this.objects.Remove(oc);
            }
        }
        private void Dispose(bool dispoing)
        {
            if (disposed == false)
            {
                if (dispoing)
                {
                    foreach (var oc in this.objects)
                    {
                        (oc.Instance as IDisposable)?.Dispose();
                    }
                }
                this.disposed = true;
            }
        }
        ~StKObjectCollection()
        {
            this.Dispose(false);
        }
    }
}
