using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Daem0n.DI
{
    public class TypeRelationDictionary : Dictionary<Type, List<Type>>
    {
        public bool Add(Type tSource, Type tTarget)
        {
            //if (tSource != tTarget &&
            //    (tTarget.IsInterface || tSource.IsAssignableFrom(tTarget) == false))
            //{
            //    throw new IllegalTypeException(tSource, tTarget);
            //}
            if (this.ContainsKey(tSource))
            {
                if (base[tSource].Contains(tTarget))
                {
                    return false;
                }
                else
                {
                    base[tSource].Add(tTarget);
                    return true;
                }
            }
            else
            {
                this.Add(tSource, new List<Type>() { tTarget });
                return true;
            }
        }
        public bool ContainsRelation(Type tSrouce, Type tTarget)
        {
            if (this.ContainsKey(tSrouce) == false)
            {
                return false;
            }
            if (base[tSrouce].Contains(tTarget) == false)
            {
                return false;
            }
            return true;
        }
        public new Type this[Type tSource]
        {
            get
            {
                return base[tSource].First();
            }
        }
    }
}
