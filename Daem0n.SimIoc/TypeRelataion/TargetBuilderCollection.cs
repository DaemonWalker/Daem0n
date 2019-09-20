using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.SimIoc.TypeRelataion
{
    public class TargetBuilderCollection<TKey>
    {
        private ConcurrentDictionary<TKey, Func<object>> builders;
        public object GetObj(TKey tTarget) => builders[tTarget]?.Invoke();
        public void Add(TKey tTarget, Func<object> func)
        {
            if (builders.TryAdd(tTarget, func) == false)
            {
                throw new Exception($"{this.GetType()} Thread Error");
            }
        }

    }
}
