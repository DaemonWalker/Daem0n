using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daem0n.SimIoc.TypeRelataion
{
    internal class ImplementationRelation
    {
        private ConcurrentDictionary<Type, IEnumerable<Type>> relation = new ConcurrentDictionary<Type, IEnumerable<Type>>();
        private ConcurrentDictionary<Type, BuilderInfo> builders = new ConcurrentDictionary<Type, BuilderInfo>();


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

        public void AddBuilder(Type tTarget, Func<IServiceProvider, object> func, object instance)
        {
            if (builders.ContainsKey(tTarget))
            {
                return;
            }
            if (builders.TryAdd(tTarget, new BuilderInfo(instance, func)) == false)
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
        public BuilderInfo GetBuilder(Type tTarget) => builders[tTarget];

        public void Add(Type tSource, Type tTarget, Func<IServiceProvider, object> builder, object instacne)
        {
            this.AddRelation(tSource, tTarget);
            this.AddBuilder(tTarget, builder, instacne);
        }

        private IEnumerable<Type> Get(Type tSource)
        {
            if (this.relation.TryGetValue(tSource, out var list) == false)
            {
                throw new Exception("Thread Err");
            }
            return list;
        }
        internal List<string> Contains(string name)
        {
            var list = this.relation.Where(_ => _.Key.Name.Contains(name));
            var r = new List<string>();
            foreach (var item in list)
            {
                foreach (var v in item.Value)
                {
                    r.Add($"{item.Key.FullName} -> {v.FullName}");
                }
            }
            return r;
        }
    }
    internal sealed class BuilderInfo
    {
        public object Instance { get; private set; }
        public Func<IServiceProvider, object> Builder { get; private set; }
        public BuilderInfo(object instance, Func<IServiceProvider, object> builder)
        {
            this.Instance = instance;
            this.Builder = builder;
        }
    }
}
