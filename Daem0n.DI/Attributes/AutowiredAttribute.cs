using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.DI
{
    public class AutowiredAttribute : Attribute
    {
        public Type TargetType { get; set; }
        public Type SourceType { get; set; }
        public ServiceLifetime RegistionWay { get; set; }
    }
}
