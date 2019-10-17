using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Daem0n.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var l = Enumerable.Range(1, 9);
            Console.WriteLine(l.FirstOrDefault(_ => _ / 100 > 1));
        }
    }
    class A
    {
        public A() { }
        public A(string a) { }
        public A(int a) { }
    }
}
