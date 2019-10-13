using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Daem0n.Test.WebApi.Models
{
    public class Foo : IFoo
    {
        public Foo(SingletonObject singletonObject)
        {
            this.Name = singletonObject.StringValue;
            this.ID = Guid.NewGuid().ToString();
        }
        public string Name { get; }
        public string ID { get; }
    }
}
