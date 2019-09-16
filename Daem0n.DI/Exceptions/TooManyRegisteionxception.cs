using System;
using System.Collections.Generic;
using System.Text;

namespace Daem0n.DI
{
    public class TooManyRegisteionxception : Exception
    {
        public TooManyRegisteionxception(Type type, string registedWay)
        {
            this.RegistingType = type;
            this.RegistedWay = registedWay;
        }
        public Type RegistingType { get; }
        public string RegistedWay { get; }
    }
}
