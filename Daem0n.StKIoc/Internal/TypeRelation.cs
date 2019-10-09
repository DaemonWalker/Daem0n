using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daem0n.StKIoc.Internal
{
    class TypeRelationCollection
    {
        private ConcurrentDictionary<Type, List<TypeRecord>> dict = new ConcurrentDictionary<Type, List<TypeRecord>>();
        public TypeRecord Add(Type service, Type implement, ServiceLifetime lifetime)
        {
            var list = dict.GetOrAdd(service, new List<TypeRecord>());
            list.Add(new TypeRecord(lifetime, implement));
            return list.Last();
        }
        public TypeRecord Get(Type service)
        {
            dict.TryGetValue(service, out var list);
            return list?.First();
        }
        public List<TypeRecord> GetAll(Type service)
        {
            dict.TryGetValue(service, out var list);
            return list;
        }
        public bool Contains(Type service)
        {
            return this.Get(service) != null;
        }
    }
}
