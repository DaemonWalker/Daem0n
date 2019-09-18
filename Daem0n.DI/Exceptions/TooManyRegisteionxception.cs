using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.DI
{
    public class TooManyRegisteionxception : Exception
    {
        public TooManyRegisteionxception(Type type, ServiceLifetime registedWay)
        {
            this.RegistingType = type;
            this.RegistedWay = registedWay;
        }
        public Type RegistingType { get; }
        public ServiceLifetime RegistedWay { get; }
    }
}
