using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.DI
{
    internal class TargetObjectContainer : Dictionary<Type, ObjectBuilder>
    {
        public new object this[Type t]
        {
            get
            {
                return GetObject(t);
            }
        }
        public object GetObject(Type t)
        {
            lock (t)
            {
                if (base[t].Obj == null)
                {
                    if (base[t].Func != null)
                    {
                        base[t].Obj = base[t].Func();
                    }
                }
            }
            return base[t].Obj;
        }
        public void Add(Type t, object obj)
        {
            base.Add(t, new ObjectBuilder() { Obj = obj });
        }
        public void Add(Type t, Func<object> func)
        {
            if (base.ContainsKey(t))
            {
                if (base[t].Func != func)
                {
                    //Console.WriteLine(func + "\n" + base[t].Func);
                    //throw new InvalidOperationException("Too Many Func");
                }
            }
            else
            {
                base.Add(t, new ObjectBuilder() { Func = func });

            }
        }

    }
    
}
