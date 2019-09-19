using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.DI
{
    internal class ObjectBuilder
    {
        public object Obj { get; set; }
        public Func<object> Func { get; set; }
    }
}
