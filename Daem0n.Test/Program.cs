using System;

namespace Daem0n.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var t = typeof(A);
            var cs = t.GetConstructors();
        }
    }
    class A
    {
        public A() { }
        public A(string a) { }
        public A(int a) { }
    }
}
