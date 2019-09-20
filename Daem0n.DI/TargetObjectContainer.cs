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
                    base[t].Func = func;
                }
            }
            else
            {
                base.Add(t, new ObjectBuilder() { Func = func });

            }
        }
        public object CreateObj(Type t)
        {
            if (base.ContainsKey(t) && base[t].Func != null)
            {
                return base[t].Func();
            }
            return null;
        }
    }

}
