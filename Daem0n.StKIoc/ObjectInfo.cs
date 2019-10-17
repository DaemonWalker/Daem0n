using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.StKIoc
{
    public class ObjectInfo : IComparer<ObjectInfo>
    {
        public String ObjectType { get; internal set; }
        public string Lifetime { get; internal set; }
        public string SerivceScoped { get; internal set; }

        public int Compare(ObjectInfo x, ObjectInfo y)
        {
            if (x.Lifetime != y.Lifetime)
            {
                return x.Lifetime.CompareTo(y.Lifetime);
            }
            else if (x.ObjectType != y.ObjectType)
            {
                return x.ObjectType.CompareTo(y.ObjectType);
            }
            else return x.SerivceScoped.CompareTo(y.SerivceScoped);
        }
    }
}
