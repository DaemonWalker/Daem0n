using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daem0n.SimIoc.TypeRelataion
{
    public class ImplementitionRelation
    {
        private ConcurrentDictionary<Type, IEnumerable<Type>> relation;
        private ConcurrentDictionary<Type, Func<object>> builders;


        public bool Contains(Type tSource)
        {
            if (tSource == null)
            {
                throw new ArgumentNullException($"{this.GetType()}-Contains");
            }
            return relation.ContainsKey(tSource);
        }
        public bool Contains(Type tSource, Type tTarget)
        {
            var result = Contains(tSource);
            if (tTarget == null || result == false)
            {
                return result;
            }

            var list = Get(tSource);
            return list.Contains(tTarget);
        }

        public Type GetTarget(Type tSource)
        {
            if (Contains(tSource) == false)
            {
                return null;
            }
            var list = Get(tSource);
            return list.FirstOrDefault();
        }

        public void AddBuilder(Type tTarget, Func<object> func)
        {
            if (builders.TryAdd(tTarget, func) == false)
            {
                throw new Exception($"{this.GetType()} Thread Error");
            }
        }
        public void AddRelation(Type tSource, Type tTarget)
        {
            relation.AddOrUpdate(tSource,
                new List<Type>() { tTarget },
                (t, list) => list.Append(t));
        }
        public object GetObj(Type tTarget) => builders[tTarget]?.Invoke();

        public void Add(Type tSource, Type tTarget, Func<object> builder)
        {
            this.AddRelation(tSource, tTarget);
            this.AddBuilder(tTarget, builder);
        }

        private IEnumerable<Type> Get(Type tSource)
        {
            if (this.relation.TryGetValue(tSource, out var list) == false)
            {
                throw new Exception("Thread Err");
            }
            return list;
        }
    }
}
