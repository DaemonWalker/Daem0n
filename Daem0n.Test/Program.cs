using Daem0n.DI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

namespace Daem0n.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(string.Format(CultureInfo.InvariantCulture, "Configure{0}Services", "BALABLA"));
        }
    }
    class A
    {
        public A() { }
        public A(string a) { }
        public A(int a) { }
    }
}
